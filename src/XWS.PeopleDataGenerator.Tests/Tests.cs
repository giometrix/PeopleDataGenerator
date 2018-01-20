using System;
using System.Linq;
using Xunit;

namespace XWS.PeopleDataGenerator.Tests
{
	public class Tests
	{
		[Fact]
		public void default_settings_generate_1000_records()
		{
			var clg = new ContactListGenerator();
			var list = clg.Generate();
			Assert.Equal(1002, list.Split('\n').Length);
		}

		[Fact]
		public void custom_number_of_rows_is_honored()
		{
			var clg = new ContactListGenerator();
			clg.NumberOfLinesToGenerate = 5;
			var list = clg.Generate();
			Assert.Equal(7, list.Split('\n').Length);
		}

		[Fact]
		public void generate_list_with_custom_data()
		{
			var clg = new ContactListGenerator();
			clg.NumberOfLinesToGenerate = 5;
			var list = clg.Generate(",UID", () => "," + Guid.NewGuid());
			var lines = list.Split('\n');
			Assert.True(lines[0].Replace("\r", "").EndsWith(",UID"));	
			var lastColumn = lines[1].Split(',').Last().Replace("\r","");
			Assert.True(Guid.TryParse(lastColumn, out _ ));
		}

		[Fact]
		public void demonstrate_custom_generator()
		{
			var clg = new ContactListGeneratorWithCustomerId();
			clg.NumberOfLinesToGenerate = 5;
			var list = clg.Generate();
			var lines = list.Split('\n');
			Assert.True(lines[0].Replace("\r", "").EndsWith("Customer Id"));
			var lastColumn = lines[1].Split(',').Last().Replace("\r", "");
			Assert.True(int.TryParse(lastColumn, out _));
		}
	}
}
