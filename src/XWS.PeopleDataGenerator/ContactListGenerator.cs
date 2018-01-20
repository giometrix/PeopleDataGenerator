using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XWS.PeopleDataGenerator
{
	public class ContactListGenerator
	{

		private List<AreaCode> _areaCodes = new List<AreaCode>();

		//Some popular email domains

		private List<string> _firstNames = new List<string>();
		private List<string> _lastNames = new List<string>();
		private List<PostalCode> _postalCodes = new List<PostalCode>();
		private List<State> _states = new List<State>();
		private List<State> _weightedStates = new List<State>();

		public ContactListGenerator()
		{
			NumberOfLinesToGenerate = 1000;
		}

		public int NumberOfLinesToGenerate { get; set; }

		/// <summary>
		///     List of first names.
		/// </summary>
		protected List<string> FirstNames
		{
			get
			{
				lock (_firstNames)
				{
					if (_firstNames.Count == 0) _firstNames = GetFirstNames();
				}

				return _firstNames;
			}
		}

		/// <summary>
		///     List of last names.
		/// </summary>
		protected List<string> LastNames
		{
			get
			{
				lock (_lastNames)
				{
					if (_lastNames.Count == 0) _lastNames = GetFirstNames();
				}

				return _lastNames;
			}
		}

		/// <summary>
		///     List of North American area codes
		///     <example>
		///         718, 212
		///     </example>
		/// </summary>
		protected List<AreaCode> AreaCodes
		{
			get
			{
				lock (_areaCodes)
				{
					if (_areaCodes.Count == 0) _areaCodes = GetAreaCodes();

					return _areaCodes;
				}
			}
		}

		/// <summary>
		///     List of American zip codes
		///     <example>
		///         11739, 11725
		///     </example>
		/// </summary>
		protected List<PostalCode> PostalCodes
		{
			get
			{
				lock (_postalCodes)
				{
					if (_postalCodes.Count == 0)
						_postalCodes = GetPostalCodeList();
				}

				return _postalCodes;
			}
		}

		/// <summary>
		///     List of American states, weighted by population
		///     <example>
		///         NY, CA
		///     </example>
		/// </summary>
		protected List<State> WeightedStates
		{
			get
			{
				lock (_weightedStates)
				{
					if (_weightedStates.Count == 0)
						_weightedStates = GetWeightedStateList();
				}

				return _weightedStates;
			}
		}

		/// <summary>
		///     List of North American states
		///     <example>
		///         NY, CA
		///     </example>
		/// </summary>
		protected List<State> States
		{
			get
			{
				lock (_states)
				{
					if (_states.Count == 0)
						_states = GetStateList();
				}

				return _states;
			}
		}

		/// <summary>
		///     List of popular email domains
		///     <example>
		///         gmail.com, yahoo.com
		///     </example>
		/// </summary>
		protected List<string> EmailDomains { get; } = new List<string>
		{
			"yahoo.com",
			"hotmail.com",
			"aol.com",
			"gmail.com",
			"msn.com",
			"comcast.net",
			"sbcglobal.net",
			"bellsouth.net",
			"verizon.net",
			"earthlink.net",
			"cox.net",
			"rediffmail.com",
			"btinternet.com",
			"charter.net",
			"shaw.ca",
			"ntlworld.com"
		};

		/// <summary>
		///     Generates the random collection of people information. You can add additional columns by filling in
		///     additionalHeaderColumns (don't forget the leading comma) and a function to return the appropriate number of
		///     additional columns per record (again, don't forget the leading comma). This approach will not cover all needs for
		///     custimization, you will need to override this method in those cases.
		/// </summary>
		/// <param name="additionalHeaderColumns"> </param>
		/// <param name="additionalGenerator"> </param>
		public virtual string Generate(string additionalHeaderColumns = "", Func<string> additionalGenerator = null)
		{
			var random = new Random();
			var sb = new StringBuilder();
			
			sb.AppendLine(
				$"First Name,Last Name,Street Address 1, Street Address 2, City, State, Zip, Phone, Email{additionalHeaderColumns}");

			for (int i = 0; i < NumberOfLinesToGenerate; i++)
			{
				var firstName = GenerateFirstName(random);
				var lastName = GenerateLastName(random);
				var streetAddress1 = GenerateStreetAddress1(random);
				var streetAddress2 = GenerateStreetAddress2(random);
				var state = GenerateState(random);
				var areaCode = GenerateAreaCode(random, state);
				var phone = GeneratePhoneNumber(random);
				var email = GenerateEmail(random, firstName, lastName);
				var additonalData = "";
				if (additionalGenerator != null) additonalData = additionalGenerator();
				GenerateCity(random, state, out var zip, out var town);

				sb.AppendLine(
					$"{firstName},{lastName},{streetAddress1},{streetAddress2},{town},{state},{zip},({areaCode}) {phone}, {email}{additonalData}");
			}

			return sb.ToString();

		}

		/// <summary>
		///     Generates an email address
		/// </summary>
		/// <param name="random"> </param>
		/// <param name="firstName"> </param>
		/// <param name="lastName"> </param>
		/// <returns> </returns>
		protected virtual string GenerateEmail(Random random, string firstName, string lastName)
		{
			var domain = PickFromList(random, EmailDomains);

			var randomNumber = random.Next(10);

			string email;

			if (randomNumber < 5)
				email = firstName[0] + lastName;
			else
				email = firstName + "." + lastName;

			if (random.Next(10) >= 7) email += random.Next(20).ToString(CultureInfo.InvariantCulture);

			return string.Format("{0}@{1}", email, domain);
		}

		/// <summary>
		///     Generates a 7 digit phone number
		/// </summary>
		/// <param name="random"> </param>
		/// <returns> </returns>
		protected virtual string GeneratePhoneNumber(Random random)
		{
			return random.Next(1000000, 9999999).ToString("###-####");
		}

		/// <summary>
		///     Generates an area code, this implimentation will most likely pick an area code associated with the given state, but
		///     will randomly pick a different state every so often to make it more realistic.
		/// </summary>
		/// <param name="random"> </param>
		/// <param name="state"> </param>
		/// <returns> </returns>
		protected virtual string GenerateAreaCode(Random random, string state)
		{
			if (random.Next(10) < 9)
			{
				var areaCodes = AreaCodes.Where(a => a.State == state).ToList();
				return areaCodes[random.Next(areaCodes.Count)].Code;
			}

			return AreaCodes[random.Next(AreaCodes.Count)].Code;
		}

		/// <summary>
		///     Picks a random first name from first name list
		/// </summary>
		/// <param name="random"> </param>
		/// <returns> </returns>
		protected virtual string GenerateFirstName(Random random)
		{
			return PickFromList(random, FirstNames);
		}

		/// <summary>
		///     Picks a random last name from the last name list
		/// </summary>
		/// <param name="random"> </param>
		/// <returns> </returns>
		protected virtual string GenerateLastName(Random random)
		{
			return PickFromList(random, LastNames);
		}

		/// <summary>
		///     Picks a random state from the state list. The list is weighted, based on population
		/// </summary>
		/// <param name="random"> </param>
		/// <returns> </returns>
		protected virtual string GenerateState(Random random)
		{
			return WeightedStates[random.Next(WeightedStates.Count)].Abbreviation;
		}

		/// <summary>
		///     Picks a random zip code and city for the given state
		/// </summary>
		/// <param name="random"> </param>
		/// <param name="state"> </param>
		/// <param name="zip"> </param>
		/// <param name="town"> </param>
		protected virtual void GenerateCity(Random random, string state, out string zip, out string town)
		{
			var zipMatches = PostalCodes.Where(z => z.State.Equals(state, StringComparison.OrdinalIgnoreCase)).ToList();

			var postalCode = zipMatches[random.Next(zipMatches.Count())];
			zip = postalCode.Zip;
			town = postalCode.Town;
		}

		/// <summary>
		///     Gets the list of area codes from the embedded area code file. You can override this method if the default list
		///     doesn't suffice.
		/// </summary>
		/// <returns> </returns>
		protected List<AreaCode> GetAreaCodes()
		{
			var sw =
				new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"XWS.PeopleDataGenerator.Data.areacodes.csv"));

			var areaCodes = ReadCsv(sw, columns => new AreaCode
			{
				Code = columns[0],
				State = columns[1]
			});

			return areaCodes;
		}

		/// <summary>
		///     Gets the list of postal codes from the postal code file. You can override this method if the default list doesn't
		///     suffice.
		/// </summary>
		/// <returns> </returns>
		protected virtual List<PostalCode> GetPostalCodeList()
		{
			var sw =
				new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"XWS.PeopleDataGenerator.Data.zips_v2.csv"));

			var textInfo = new CultureInfo("en-US").TextInfo;

			var postalCodes = ReadCsv<PostalCode>(sw, columns =>
				new PostalCode
				{
					County = textInfo.ToTitleCase(columns[5].Replace("\"", "").ToLower()),
					Latitude = float.Parse(columns[1].Replace("\"", "")),
					Longitude = float.Parse(columns[2].Replace("\"", "")),
					State = columns[4].Replace("\"", ""),
					Town = textInfo.ToTitleCase(columns[3].Replace("\"", "").ToLower()),
					Zip = columns[0].Replace("\"", "")
				});

			return postalCodes;
		}

		/// <summary>
		///     Gets the list of states from the embedded states file. You can override this method if the default list doesn't
		///     suffice.
		/// </summary>
		/// <returns> </returns>
		protected virtual List<State> GetStateList()
		{
			var sw =
				new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"XWS.PeopleDataGenerator.Data.states.csv"));

			var states = ReadCsv<State>(sw, columns =>
				new State
				{
					Abbreviation = columns[1],
					Name = columns[0],
					Population = int.Parse(columns[2]),
					PercentageOfTotalPopulation = float.Parse(columns[3])
				});

			return states;
		}

		protected virtual List<State> GetWeightedStateList()
		{
			var weightedStateList = new List<State>();

			foreach (var s in States)
				for (var i = 1; i <= Math.Ceiling(s.PercentageOfTotalPopulation * 100); i++)
					weightedStateList.Add(s);

			return weightedStateList;
		}

		/// <summary>
		///     Will usually not return anything, but randomly will return a unit # of some sort
		/// </summary>
		/// <param name="random"> </param>
		/// <example>
		///     Unit 1A, Apt 2
		/// </example>
		/// <returns> </returns>
		protected virtual string GenerateStreetAddress2(Random random)
		{
			if (random.Next(10) >= 8)
			{
				//floor
				if (random.Next(10) >= 8)
					if (random.Next(10) >= 8)
					{
						var floor = random.Next(1, 140).ToString(CultureInfo.InvariantCulture);
						floor += GetNumberSuffix(floor);
						return floor + " Floor";
					}
					else //smaller building
					{
						var floor = random.Next(1, 12).ToString(CultureInfo.InvariantCulture);
						floor += GetNumberSuffix(floor);
						return floor + " Floor";
					}

				var type = PickFromList(random, new List<string> {"Apt", "Unit", "Offc"});
				var number = random.Next(1, 21).ToString(CultureInfo.InvariantCulture);
				var suffix = "";

				if (random.Next(10) >= 7) suffix = ((char) random.Next(65, 91)).ToString(CultureInfo.InvariantCulture);

				return string.Format("{0} {1}{2}", type, number, suffix);
			}

			return string.Empty;
		}

		/// <summary>
		///     Returns the appropriate suffix for a number
		/// </summary>
		/// <param name="number"> </param>
		/// <returns> </returns>
		protected virtual string GetNumberSuffix(string number)
		{
			if (number.EndsWith("1"))
				return "st";
			else if (number.EndsWith("2"))

				return "nd";

			else if (number.EndsWith("3"))
				return "rd";
			else
				return "th";
		}

		/// <summary>
		///     Generates a street address. Streets may be named or numbered. Some street types are more popular than others (e.g.
		///     St vs Cswy)
		/// </summary>
		/// <param name="random"> </param>
		/// <returns> </returns>
		protected virtual string GenerateStreetAddress1(Random random)
		{
			var streetAddress = random.Next(1, 5000).ToString(CultureInfo.InvariantCulture);
			string street;
			var type = string.Empty;

			var isNumberStreet = false;

			if (random.Next(10) < 3)
			{
				isNumberStreet = true;
				var number = random.Next(1, 301);
				street = number.ToString(CultureInfo.InvariantCulture);

				street += GetNumberSuffix(street);
			}
			else
			{
				street = PickFromList(random, LastNames);
			}

			if (random.Next(10) > 7 && !isNumberStreet)
				type = PickFromList(random,
					new List<string> {"Blvd", "Cswy", "Conc", "Ct", "Crsg", "Jctn", "Pkwy", "Pl", "Trnpk", "Wy"});
			else
				type = random.Next(2) == 0 ? "St" : "Ave";

			return string.Format("{0} {1} {2}", streetAddress, street, type);
		}

		/// <summary>
		///     Picks an item randomly from a list
		/// </summary>
		/// <typeparam name="T"> </typeparam>
		/// <param name="random"> </param>
		/// <param name="list"> </param>
		/// <returns> </returns>
		protected T PickFromList<T>(Random random, List<T> list)
		{
			return list[random.Next(0, list.Count)];
		}

		/// <summary>
		///     Gets the list of last names from the embedded last names file. You can override this method if the default list
		///     doesn't suffice.
		/// </summary>
		/// <returns> </returns>
		protected virtual List<string> GetLastNames()
		{
			return OpenNamesFiles("XWS.PeopleDataGenerator.Data.CSV_Database_of_Last_Names.csv");
		}

		/// <summary>
		///     Gets the list of first names from the embedded first names file. You can override this method if the default list
		///     doesn't suffice.
		/// </summary>
		/// <returns> </returns>
		protected virtual List<string> GetFirstNames()
		{
			return OpenNamesFiles("XWS.PeopleDataGenerator.Data.CSV_Database_of_First_Names.csv");
		}

		/// <summary>
		///     returns a list of string from specified embedded files. Used only by first name and last name generators.
		/// </summary>
		/// <returns> </returns>
		private List<string> OpenNamesFiles(string filename)
		{
			var names = new List<string>();
			var assembly = Assembly.GetExecutingAssembly();
			using (var sw = new StreamReader(assembly.GetManifestResourceStream(filename)))
			{
				while (sw.Peek() >= 0) names.Add(sw.ReadLine());
			}

			return names;
		}

		/// <summary>
		///     Reads an open stream representing a CSV file
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="streamReader"></param>
		/// <param name="readLineHandler"></param>
		/// <returns></returns>
		protected List<T> ReadCsv<T>(StreamReader streamReader, Func<string[], T> readLineHandler)
		{
			var list = new List<T>();

			using (streamReader)
			{
				while (streamReader.Peek() >= 0)
				{
					var splitLine = streamReader.ReadLine().Split(',');
					list.Add(readLineHandler(splitLine));
				}
			}

			return list;
		}
	}
}