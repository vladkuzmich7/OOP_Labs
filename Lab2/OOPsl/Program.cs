using OOPsl.MenuFunctions;
using OOPsl.DocumentFunctions.Managers;
using OOPsl.UserFunctions;

ConsoleHelper.DisableQuickEditMode();
UserManager userManager = new UserManager();
DocumentAccessManager accessManager = new DocumentAccessManager();
DocumentManager documentManager = new DocumentManager(accessManager);
accessManager.RestoreUserDocuments(userManager, documentManager);
IMenu userMenu = new ConsoleMenu(userManager, documentManager, accessManager);
ApplicationMenu appMenu = new ApplicationMenu(userMenu);
appMenu.Run();