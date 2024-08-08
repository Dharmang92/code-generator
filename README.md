# CodeGenerator

Generates Endpoints, Models and Services for C# Application using Liquid templates.

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
## Below are some available variables you can use to customize the inputs further.
- {{ EntityName }} - Corresponds to the 'Entity Name' field
- {{ ModelPath }} - Corresponds to the 'Model Namespace' field
 
![image](https://github.com/user-attachments/assets/99b41e36-f064-443a-8c84-4d1c3f1b8d6f)
