# My Website is Compromised, What Now?
If your website has been compromised do not worry! No one has access to all of your secrets unless they use the Azure key vault API directly. 

You can prevent them from doing this with quick action!

1. Log into the Azure Portal [https://azure.microsoft.com/en-us/](https://azure.microsoft.com/en-us/)
2. Navigate to the `Azure Active Directory` Blade
3. Select `App Registrations`
4. Select the App Registration you created for your DNN Website
5. Search and select `Certificates & Secrets`

At this window just delete your client secret and generate a new one. 

## Restrict Access
If your website has been compromised and generating a new secret key isn't good enough for your security policies. You can easily just delete the app registration and create a new one. 

To accomplish this follow the [Getting Started](GETTING-STARTED.md)