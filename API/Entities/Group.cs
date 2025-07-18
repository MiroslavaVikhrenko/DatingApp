using System;
using System.ComponentModel.DataAnnotations;

namespace API.Entities;

// to ensure scalability => if we have several servers, instances of app => store all info in db not in memory of individual server
public class Group(string name)
{
    [Key] // PK instead of Id as name is unique
    public string Name { get; set; } = name;

    // navigation properties 
    public ICollection<Connection> Connections { get; set; } = [];
}
