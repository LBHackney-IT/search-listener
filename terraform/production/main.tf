# INSTRUCTIONS:
# 1) ENSURE YOU POPULATE THE LOCALS
# 2) ENSURE YOU REPLACE ALL INPUT PARAMETERS, THAT CURRENTLY STATE 'ENTER VALUE', WITH VALID VALUES
# 3) YOUR CODE WOULD NOT COMPILE IF STEP NUMBER 2 IS NOT PERFORMED!
# 4) ENSURE YOU CREATE A BUCKET FOR YOUR STATE FILE AND YOU ADD THE NAME BELOW - MAINTAINING THE STATE OF THE INFRASTRUCTURE YOU CREATE IS ESSENTIAL - FOR APIS, THE BUCKETS ALREADY EXIST
# 5) THE VALUES OF THE COMMON COMPONENTS THAT YOU WILL NEED ARE PROVIDED IN THE COMMENTS
# 6) IF ADDITIONAL RESOURCES ARE REQUIRED BY YOUR API, ADD THEM TO THIS FILE
# 7) ENSURE THIS FILE IS PLACED WITHIN A 'terraform' FOLDER LOCATED AT THE ROOT PROJECT DIRECTORY

terraform {
    required_providers {
        aws = {
            source  = "hashicorp/aws"
            version = "~> 3.0"
        }
    }
}

provider "aws" {
    region = "eu-west-2"
}

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

locals {
    parameter_store = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter"
}

terraform {
  backend "s3" {
    bucket  = "terraform-state-housing-development"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/housing-search-listener/state"
  }
}

resource "aws_sqs_queue" "housing_search_listener_queue" {
  name                        = "housingsearchlistenerqueue.fifo"
  fifo_queue                  = true
  content_based_deduplication = true
  kms_master_key_id = "alias/aws/sqs"
}

resource "aws_sqs_queue_policy" "housing_search_listener_queue_policy" {
  queue_url = aws_sqs_queue.housing_search_listener_queue.id
  policy = <<POLICY
  {
      "Version": "2012-10-17",
      "Id": "sqspolicy",
      "Statement": [
      {
          "Sid": "First",
          "Effect": "Allow",
          "Principal": "*",
          "Action": "sqs:SendMessage",
          "Resource": "${aws_sqs_queue.housing_search_listener_queue.arn}",
          "Condition": {
          "ArnEquals": {
              "aws:SourceArn": "${aws_sqs_queue.housing_search_listener_queue.arn}"
          }
          }
      }
      ]
  }
  POLICY
}

resource "aws_sns_topic_subscription" "housing_search_listener_queue_subscribe_to_person_sns" {
  topic_arn = "arn:aws:sns:eu-west-2:364864573329:person.fifo"
  protocol  = "sqs"
  endpoint  = aws_sqs_queue.housing_search_listener_queue.arn
}

resource "aws_ssm_parameter" "housing_search_listeners_sqs_queue_arn" {
  name  = "/sqs-queue/production/housing_search_listener_queue/arn"
  type  = "String"
  value = aws_sqs_queue.housing_search_listener_queue.arn
}