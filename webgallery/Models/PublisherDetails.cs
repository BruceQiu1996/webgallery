
using System;

namespace webgallery.Models
{

    public class PublisherDetails 
    {
        
        public int SubmissionID { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerAddress1 { get; set; }
        public string OwnerAddress2 { get; set; }
        public string OwnerAddress3 { get; set; }
        public string OwnerCity { get; set; }
        public string OwnerCountry { get; set; }
        public string OwnerMiddleName { get; set; }
        public string OwnerState { get; set; }
        public string OwnerZipCode { get; set; }
        public string OwnerTitle { get; set; }
        public string OwnerSuffix { get; set; }
        public int OwnerSubmitterID { get; set; }
        public string OwnerPrefix { get; set; }
       
        public PublisherDetails()
        {

        }

        public PublisherDetails(int id , int submitterid, string title, string prefix, string suffix, string firstname, string lastname, string middlename, string email, string address1, string address2, string address3 , string city, string country, string state, string zipcode)
        {
            SubmissionID = id;
            OwnerSuffix = suffix;
            OwnerPrefix = prefix;
            OwnerFirstName = firstname;
            OwnerLastName = lastname;
            OwnerMiddleName = middlename;
            OwnerEmail = email;
            OwnerCountry = country;
            OwnerCity = city;
            OwnerAddress1 = address1;
            OwnerAddress2 =  address2;
            OwnerAddress3 = address3;
            OwnerState = state;
            OwnerSubmitterID = submitterid;
            OwnerTitle = title;
            OwnerZipCode = zipcode;
            


        }
    }
}