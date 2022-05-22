using System;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EF.Tests.Common;
using EF.Tests.Dtos;
using EF.Tests.Model;
using Maddalena;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EF.Tests;

public class InMemoryMappingTests
{
    [Fact]
    public void Test()
    {
        var countries = Enum.GetValues<CountryCode>()
            .Select(x => (Code: x.ToString(), Country: Country.FromCode(x).OfficialName))
            .ToDictionary(x => x.Code);
        
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDto>()
            //.ForMember(dto => dto.Country, src => src.MapFrom(x => x.CountryCode == "JP" ? "Japan" : "Czech Republic")));
            .ForMember(dto => dto.Country, src => src.MapFrom(x => countries[x.CountryCode])));
        
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql("host=localhost;database=testdb;user id=postgres;password=postgres;")
            .LogTo(Console.WriteLine)
            .Options;

        var dbContext = TestDbContextFactory.Create(options);
        dbContext.Persons.AddRange(
            new Person
            {
                FirstName = "Петр",
                CountryCode = "CZ"
            },
            new Person
            {
                FirstName = "Иван",
                CountryCode = "JP"
            });

        dbContext.SaveChanges();

        var result = dbContext.Persons
            .ProjectTo<PersonDto>(config)
            .ToArray();
    }
}