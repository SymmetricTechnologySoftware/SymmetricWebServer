Hi, 
I have included an example database called Northwind from the repo at https://northwinddatabase.codeplex.com/

How to install
===========================

1) Install a version of Microsoft SQL Server 2005 or greater
2) Execute the "northwind.sql" script on the server
3) Copy the "content.db" file to the "Web_Content" folder in your Symmetric Web Server application folder
4) Log in on your Symmetric Web Server as an Admin
5) Go to admin panel > reporting > manage connections
6) Edit the Northwind connection entry and change the server details to the one you have setup the northwind database on (Remember to select the northwind database).
7) You now should have a working reporting server!
8) Go to view reports to see the avaliable reports