# Carbonara Recognizer - IaC

This project contains the bicep templates you can use to create the environment to host the Carbonara Recognizer project.

To create the resource group and to deploy the resources you need for the project, simply run the following command:

```
az deployment sub create --location <region> --template-file main.bicep --parameters customVisionProjectId=<projectId>
```

where 
- `<region>` is the location where you want to create the deployment
- `<projectId>` is the project Id of your custom vision project. You can find it in the custom vision portal.


You can also set these parameters:

- `location` : the location you want to deploy the resources (by default the location is the same of your deployment)
- `resourceGroupName` : the name of the resource group (by default its value is `ServerlessFacesAnalyzer-rg`)
- 

```
az deployment sub create --location <region> --template-file main.bicep --parameters location=<resourceRegion> resourceGroupName=<rgName> customVisionProjectId=<projectId>
```