# Getting Started Guide (Azure Key Vault)

The DNN Key Master requires a few things outside of DNN to be properly configured

* Azure Account
* Azure App Registration
* Azure Key Vault

## Azure Account
You will need to create an Azure Account, which is easily done at [https://azure.microsoft.com/en-us/](https://azure.microsoft.com/en-us/).

## Azure App Registration
The DNN Key Master relies heavily on an Azure App Registration to communicate with the Azure Key Vault. This provides a powerful security mechanism in case your website is compromised. You can easily go and turn off access to your key vault and no one will be able to access the keys anymore. More details on this can be found at our (I've Been Compromised Guide)[COMPROMISED-WEBSITE.md]

Create your App Registration:

1. Log into the Azure Portal [https://azure.microsoft.com/en-us/](https://azure.microsoft.com/en-us/)
2. Select `Azure Active Directory`
3. In the Azure Active Directory Blade search and select `App Registrations`
4. Create a new registration

### App Registration Options:

* Name: this is anything you want it to be
* Supported Account Types: Select 'Accounts in this organizational directory only'

Select the Register button at the bottom and Azure will start creating your App Registration

### Gather Secrets
Once your app has been created you will need to gather your secrets to be used in the DNN Key Master. 

1. Navigate to the `Azure Active Directory` blade and select `App Registrations`
2. Select the App you created earlier

Now you can access the following identifiers:

* Application (Client ID)
* Directory (Tenant ID)

Copy these down for use later.

Create secret key:
1. Select `Certificates & Secrets`
2. Select `New Client Secret`

Save the generated client secret as this is the master password for your app registration, without nothing will work!

## Azure Key Vault
Creating the Azure Key Vault and providing access is the last step for configuring azure

1. Log into the Azure Portal [https://azure.microsoft.com/en-us/](https://azure.microsoft.com/en-us/)
2. Click Add New Resource
3. Search for `Key Vault`
4. Specify the name and resource group of your Key Vault and select create

Copy down your Key Vault URL as you will need that to configure the DNN Key Master.

### Add App Registration to Key Vault

1. Navigate to the `Key Vault` blade that you just created
2. Search and select `Access Policies`
3. Select `Add New`
4. Under `Select Principle` search for the name you specified earlier for your App Registration and click on Select
5. Under Secret Permissions, select: `Get`, `List`, `Set` and `Delete`
6. Select OK
7. Select Save

### Key Vault Secret Permissions
For the DNN Key Master to function correctly you will need to have the following permissions for your App Registration

* Get
* List
* Set
* Delete

## Installing the DNN Key Master
Once Azure is all configured you can install the latest build and start protecting your application secrets

1. Get the latest [installer](https://github.com/HoeflingSoftware/Dnn.KeyMaster/releases)
2. Install the extension into your DNN Website
3. Using the persona bar go to Settings -> Key Master
4. Enter all the secrets you obtained from creating everything in Azure

* ClientID or ApplicationID (they are the same)
* DirectoryID or TenantID (they are the same)
* Client Secret
* Key Vault URL
* Secret Name - This is a unique name you come up with that prefixes your secrets in the key vault

Once everything is entered you can test your secrets. 

If everything works, go ahead and click the Start button