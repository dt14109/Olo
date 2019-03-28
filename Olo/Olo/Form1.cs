using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Linq;

namespace Olo
{
    /**
     * This class contains all our Toppings
     */
    public class Topping
    {
        private string[] tops;

        public string[] toppings {
            get
            {
                return this.tops;
            }
            // We have to sort the toppings otherwise "peppers, olives" would be different than "olives, peppers"
            set
            {
                this.tops = value;
                Array.Sort(this.tops);
            }
        }

        /**
         * Return a string of all toppings joined together, for example "olives peppers"
         * We will need this for comparison
         * 
         */
        public string getAllData()
        {
            string data = "";
            foreach(var temp in this.tops)
            {
                data += temp;
                data += " ";
            }
            return data;
        }
    }

    /**
     * The Form and processing of the data
     */
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /**
         * Pushed the button
         */
        private void buttonRead_Click(object sender, EventArgs e)
        {
            string file = this.textBoxFile.Text;// The file to read
            string buffer;
            decimal items = 0;
            KeyValuePair<string, long> kvp;
            WebClient client = new WebClient();
            Stream stream = null;

            try
            {
                stream = client.OpenRead(file);
            }
            catch (WebException ex)
            {
                MessageBox.Show("Seems we had an error: " + ex.Message);
                return;
            }
            
            using (StreamReader reader = new StreamReader(stream))
            {
                // Each item is read into a Topping class
                buffer = reader.ReadToEnd();
                var toppings = new JavaScriptSerializer().Deserialize<List<Topping>>(buffer);

                // Each Topping class is now sorted alphabetically.  So we need a count of each topping(s).  
                // We will create a Dictionary<key, value> where the key are the toppings joined together 
                // in one string and the value is the number of times we have those toppings
                Dictionary<string, long> dictionary = new Dictionary<string, long>();
                foreach(Topping topping in toppings)
                {
                    buffer = topping.getAllData();
                    if (dictionary.ContainsKey(buffer))
                    {
                        long l = dictionary[buffer];
                        l++;
                        dictionary.Remove(buffer); // Remove it before putting it back in
                        dictionary.Add(buffer, l);
                    }
                    else
                    {
                        // This is the first time these toppings are added
                        dictionary.Add(buffer, 1);
                    }
                }

                // Sort the dictionary by values so that we have them in order 
                var orderedDictionary = dictionary.OrderByDescending(x => x.Value);

                // How many items do we want to display?  20 by default
                // But what if there are only 15 items?
                if (orderedDictionary.Count() < this.numericUpDown1.Value)
                {
                    MessageBox.Show("Error", "You are asking for " + this.numericUpDown1.Value + " items.  But there are only " + orderedDictionary.Count() + " items.");
                    items = orderedDictionary.Count();
                }
                else
                {
                    items = this.numericUpDown1.Value;
                }

                // Display order: rank(most popular at top)  number of times ordered  toppings  
                for (int i = 0; i < items; i++)
                {
                    kvp = orderedDictionary.ElementAt(i);
                    buffer = String.Format("{0}\t{1}\t\t{2}", i + 1, kvp.Value, kvp.Key);
                    this.listBox1.Items.Add(buffer);
                }
            }
        }

        /**
         * Close the window
         */
        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
