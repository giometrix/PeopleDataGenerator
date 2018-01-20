# People Data Generator
Need to load a bunch of test data to represent people (e.g. name, address, phone, etc.)? Wish it looked realistic? People Data Generator is what you need.

## Note
This library & utility was originally hosted on the now defunct codeplex ( https://peopledatagenerator.codeplex.com/ ), and is being moved
to github before codeplex goes away forever.  The code has not been modified in years, though before pushing I made some minor changes
to make it more suitable for a nuget package.

## Features

* Realistic names
* Realistic addresses, using real towns and postal codes
* Realistic phone numbers and emails
* Very Extensible via inheritance or delegates, so that data generation can match your needs
* Generated data is weighted to make it more realistic, e.g. "St" and "Ave" will be generated far more often in street addresses than "Ct" or "Cswy." More people will be generated from populated states like California than low populated states like Alaska. If you don't like this behavior you can always override it in child classes.

#### Sample Generated Data (By default)

![Sample Generated Data](https://i.imgur.com/tkjA7vi.png "Sample Generated Data (By default)")

### Data Sources

Name list: http://www.quietaffiliate.com/free-first-name-and-last-name-databases-csv-and-sql
Town and zip codes: http://sujee.net/tech/articles/geocoded/
Area code list: http://www.bennetyee.org/ucsd-pages/area.html
State population list (used for weighting): http://en.wikipedia.org/wiki/ListofU.S.statesandterritoriesby_population

## Extending/Modifying Data Generator Behavior:

There are a few ways of extending or modifying generator behavior.  The simplest way requires no new classes.  If you simply want to add a few more columns, and those columns do not rely on any other person's data (e.g.  requiring sequential ids) then you can simply specify the additional column headers and a lambda to return the additional columns.  Don't forget the leading comma!

Example:

Add a new column to the default output called UID.  UID will be a GUID.

```csharp 
generator.GenerateFile(",UID", ()=> "," + Guid.NewGuid());
```

The above does not cover all scenarios however.  If you need to

* rely on previous rows of data (e.g. sequential id)
* change one or more data source (e.g. state list)
* change the output of the generated file

then you should inherit from ContactListGenerator and override the appropriate methods and/or properties.

Below are some examples:

Generate sequential Ids.  Implementation is almost the same as the base class's, except we add an extra column that writes out the sequential number, which is used as an id.

```csharp
public class ContactListGeneratorWithCustomerId : ContactListGenerator
	{
		public override string Generate(string additionalHeaderColumns = "",
			Func<string> additionalGenerator = null)
		{
			var random = new Random();
			var sb = new StringBuilder();
		
				sb.AppendLine(
				"First Name,Last Name,Street Address 1, Street Address 2, City, State, Zip, Phone,Email, Customer Id" + additionalHeaderColumns);


			for (int i = 0; i < NumberOfLinesToGenerate; i++)
			{
				var firstName = GenerateFirstName(random);
				var lastName = GenerateLastName(random);
				var streetAddress1 = GenerateStreetAddress1(random);
				var streetAddress2 = GenerateStreetAddress2(random);
				var state = GenerateState(random);
				string zip;
				string town;
				var areaCode = GenerateAreaCode(random, state);
				var phone = GeneratePhoneNumber(random);
				var email = GenerateEmail(random, firstName, lastName);
				var additonalData = "";
				if (additionalGenerator != null)
				{
					additonalData = additionalGenerator();
				}
				GenerateCity(random, state, out zip, out town);
				sb.AppendLine(
					$"{firstName},{lastName},{streetAddress1},{streetAddress2},{town},{state},{zip},({areaCode}) {phone}, {email}, {i + 1000}{additonalData}");
	
			
			}

			return sb.ToString();


			}
		}
  ```
