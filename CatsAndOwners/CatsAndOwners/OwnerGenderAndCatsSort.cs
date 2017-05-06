using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CatsAndOwners
{
    /// <summary>
    /// Entity class to store Pet information
    /// </summary>
    public class Pet
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    /// <summary>
    /// Entity class to store Owner information
    /// </summary>
    public class Owner
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public List<Pet> Pets { get; set; }
    }

    /// <summary>
    /// Entity class to store Pet and Owner information
    /// </summary>
    public class PetAndOwner
    {
        public string PetName { get; set; }
        public string OwnerGender { get; set; }
    }

    /// <summary>
    /// Main Class for sorting owner genders and their cats.
    /// </summary>
    public class OwnerGenderAndCatsSort
    {
        static HttpClient client = new HttpClient();

        public static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("http://agl-developer-test.azurewebsites.net/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                List<Owner> owners = await GetOwnersAsync("people.json");                
                List<PetAndOwner> orderPetOwnerSet = SortGenderAndCats(owners);               
                DisplayOwnerGenderAndCats(orderPetOwnerSet);  
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Function to get the owners list from the web service 
        /// </summary>
        /// <param name="path">Relative Uri</param>
        /// <returns>A List of type Owner is returned</returns>
        static async Task<List<Owner>> GetOwnersAsync(string path)
        {
            List<Owner> owners = null;
            HttpResponseMessage response = await client.GetAsync(path);
            
            if (response.IsSuccessStatusCode)
            {
                // Read Json string and Deserialize it into <List<Owner>> object.
                string petsandownersJsonString = await response.Content.ReadAsStringAsync();
                owners = JsonConvert.DeserializeObject<List<Owner>>(petsandownersJsonString);
            }
            return owners;
        }

        /// <summary>
        /// Function to sort the owner genders and their cats
        /// </summary>
        /// <param name="owners">The input Owner list to be sorted</param>
        /// <returns>A List of type PetAndOwner is returned</returns>
        static List<PetAndOwner> SortGenderAndCats(List<Owner> owners)
        {
            // Eliminate owners with no pets.
            var ownersWithPets = owners.ToList()
                                           .Select(n => n)
                                           .Where(n => n.Pets != null).ToList();
            
            // Eliminate pets other than cats
            ownersWithPets.ToList().ForEach(x => x.Pets.RemoveAll(s => s.Type != "Cat"));

            // List the Owner genders and cat names
            var petOwnerSet = ownersWithPets.ToList()
                                            .SelectMany(x => x.Pets.ToList()
                                            .Select(s => new PetAndOwner { OwnerGender = x.Gender, PetName = s.Name }))
                                            .ToList();
            // Order list by Owner Gender, and then by Cat names
            var orderPetOwnerSet = petOwnerSet.OrderBy(x => x.OwnerGender)
                                               .ThenBy(x => x.PetName)
                                               .Select(x => x).ToList();
            return orderPetOwnerSet;
        }

        /// <summary>
        /// Function for displaying the owner genders and cats
        /// </summary>
        /// <param name="displayPetOwnerSet">A List of Owner genders and cat names for displaying</param>
        static void DisplayOwnerGenderAndCats(List<PetAndOwner> displayPetOwnerSet)
        {
            string previousgender = string.Empty;

            // Display Cat genders grouped by Owner names
            foreach (var petowneritem in displayPetOwnerSet)
            {
                if (petowneritem.OwnerGender == previousgender)
                {
                    Console.WriteLine(petowneritem.PetName);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(petowneritem.OwnerGender);
                    Console.WriteLine("--------------");
                    Console.WriteLine(petowneritem.PetName);
                }
                previousgender = petowneritem.OwnerGender;
            }
        }
    }
}
