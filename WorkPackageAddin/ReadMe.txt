========================================================================
    MicroStationAddInWizard : "ECApiExample" Project Overview
========================================================================

MicroStationAddInWizard has created this "ECApiExample" project for you as a starting point.

This file contains a summary of what you will find in each of the files that make up your project.

ECApiExample is the main application file this file will have an extension of cpp, cs, or
vb depending on the type of source code selected.

ECApiExampleform is a sample form sorce code page.

Keyincommands is the class for implementing the command methods.

commands.xml is the command table XML file.

ECApiExample.mke is the make file for building the application in the MicroStationV8 XM Edition
Developer shell.

/////////////////////////////////////////////////////////////////////////////
Other notes:

The webchat code is a sample of using websockets to communicate.  This was done only 
as a proof of concept and should not be considered for long term code.

There is a large chunk of code that is "example" code used to show how to work with 
ECApi.  Key points are:
ECApiExample -  has the open and close connection.
				has the locate schema code
				has code to unpack a schema stored in a dll
				Other general managed code addin application utility methods.

LocateClass -	has code to query the datastore.  (these need to be reviewed for optimization as the doc are limited.)
ClassesToFind - has the form to find a class based on the properties.
QueryForm -		uses the LocateClass code to build a list of instances that meet a query.
ElementList -	is a datagrid that will show a collection of elements.  double clicking on an entry will
					locate the element and bring up a form with the class details.
TagItemSet, DataInfo, and instancePairs are all utility classes to help the collection classes that
					work with the datagrid.
ECApiExampleLocateCmd - this will allow the user to have a predefined selection of elements or interactively select
						an element.  this will then show the details.


/////////////////////////////////////////////////////////////////////////////
