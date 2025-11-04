# BillIssue time logging tool

## Introduction

BillIssue is a tool for time logging. Currently supporting tripple herrarchy: Workspaces, Teams, and projects.

## Contents

BillIssue API - Built using C# .net

BillIssue App - Built using React

Yuniql - Database migration tooling

PostgreSQL was chosen as the target database engine.

## Local environemnt setup

Bill issue API

Prerequisites:

* .NET 8
* Redis
* PostgreSQL

Steps:

* Install yuniql CLI by running `dotnet tool install -g yuniql.cli`
* Setup the database by navigating to (your parrent path)\src\BillIssue.Api\BillIssue.Api.DatabaseMigrations
* Run yuniql setup command `yuniql run --connection-string "Your connection string" --platform "postgresql"`
* Setup connection strings in `appsettings.json` based on your environment
* Run the project, check is swagger works as expected

Bill issue App

Prerequisites:

* Node 18.20

* Navigate to the base BillIssue.App folder
* Run `npm install`
* Open .env file and make sure the `REACT_APP_API_URL` property is pointing to your API
* Run `npm start`
* Try the application working

## Features

* Timelogging to a workspace
* Workspaces can be managed in teams
* Add user to workspace with notifications
* Projects can have specific work types to log time to
* Minimal time log lookup

## Missing features at this time

* Password reset/account confirmation
