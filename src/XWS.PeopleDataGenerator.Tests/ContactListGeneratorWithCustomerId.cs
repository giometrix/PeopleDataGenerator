using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XWS.PeopleDataGenerator.Tests
{
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
	
}
