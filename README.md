# DBUpPT

DBUp is a package which manages database updates. This package adds additinoal functionality to DBUp like creating the database and creating log table.

To use it please reference package with the package manager console

`Install-Package DBUpHelperPT`

Next create Console application and in the main add following code:

```
string serverName = "ServerName";
string dbName = "DatabaseName";
DBUpHelper.DBUp dBUp = new DBUpHelper.DBUp("gc");
Assembly assembly = Assembly.GetExecutingAssembly();
dBUp.PerformUpdate(serverName, dbName, assembly, false)
```
 
Next add Scripts Folder and create inside sql scripts which you want to execute. In the properties of the script please select Embeded resource



When application will start, DBUpHelper will:

  * Check if database "DatabaseName" exists if not it will create it
  * In the database it will create schema provided in the constructor "gc"
  * It will start executing the scripts from the Script folder.

More information here http://www.productivitytools.tech/dbuphelper/
