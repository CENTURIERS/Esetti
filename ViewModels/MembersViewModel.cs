using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Esseti.ViewModels.Member;

namespace Esseti.ViewModels
{
    public partial class MembersViewModel : ViewModelBase
    {
        public override string PageTitle => "Lista członków";

        public ObservableCollection<MemberItemViewModel> Members { get; } = new();

        public MembersViewModel()
        {
            LoadSampleMembers();
        }

        private void LoadSampleMembers()
        {
            string[] firstNames = { "Kacper", "Anna", "Marek", "Zofia", "Piotr", "Ewa", "Jan", "Maria" };
            string[] lastNames = { "Kowalski", "Nowak", "Wiśniewska", "Wójcik", "Mazur", "Lewandowska" };
            string[] roles = { "Prezes", "Programista", "Skarbnik", "Członek Zarządu", "Social Media", "Rekrutacja" };
            string[] depts = { "Informatyka", "Elektronika", "Mechanika", "Zarządzanie" };

            Random rnd = new Random();

            for (int i = 1; i <= 20; i++)
            {
                string fName = firstNames[rnd.Next(firstNames.Length)];
                string lName = lastNames[rnd.Next(lastNames.Length)];
                string role = roles[rnd.Next(roles.Length)];
                string dept = depts[rnd.Next(depts.Length)];

                Members.Add(new MemberItemViewModel(
                    avatar: Array.Empty<byte>(),
                    firstName: fName,
                    lastName: lName,
                    role: role,
                    email: $"{fName.ToLower()}.{lName.ToLower()}{i}@esseti.pl",
                    collegeDepartment: dept,
                    major: "Automatyka i Robotyka",
                    joinDate: DateTime.Now.AddMonths(-rnd.Next(1, 24)).ToString("dd.MM.yyyy"),
                    isActive: i % 5 != 0 
                ));
            }
        }
    }
}
