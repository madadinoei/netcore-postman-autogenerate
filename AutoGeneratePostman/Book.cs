using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AutoGeneratePostman
{
    public class Book
    {
        public Book(DateTime date, int price, string summary, string title)
        {
            Date = date;
            Title = title;
            Price = price;
            Summary = summary;
        }

        public Book()
        {
        }

        public DateTime Date { get; set; }

        [SampleData("Sample Title Madadi")]
        [Description("dfsdasds")]
        public string Title { get; set; }

        [DefaultValue(10)]
        public int Price { get; set; }
        [DefaultValue("Summary")]

        public string Summary { get; set; }

    }
    //[AttributeUsage(AttributeTargets.Property,AllowMultiple = true)]
    [AttributeUsage(AttributeTargets.All)]

    public class SampleDataAttribute : System.Attribute
    {
        private string value;

        public SampleDataAttribute(string value)
        {
            this.value = value;
        }
    }



}
