![code-generator](https://socialify.git.ci/Dharmang92/code-generator/image?description=1&font=Inter&language=1&name=1&owner=1&pattern=Floating%20Cogs&theme=Auto)

## CodeGenerator

Generates Endpoints, Models and Services for C# Application using Liquid templates. This is assuming that ArdalisEndpoints, AutoMapper and Ardalis styled Results extensions are used. However you can modify to your custom needs (PRs are appreciated 🙂).

- Endpoints (folder)

  - List
  - GetById
  - Create
  - UpdateById

- Models (folder)

  - List.Response
  - GetById.Request
  - GetById.Response
  - Create.Request
  - UpdateById.Request
  - AutoMapper

- Services (folder)
  - IService
  - Service

# Variables

Below are some available variables you can use to customize the inputs further.

- {{ EntityName }} - Corresponds to the 'Entity Name' field
- {{ ModelPath }} - Corresponds to the 'Model Namespace' field

![image](https://github.com/user-attachments/assets/99b41e36-f064-443a-8c84-4d1c3f1b8d6f)
