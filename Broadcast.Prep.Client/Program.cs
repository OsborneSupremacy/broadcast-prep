﻿using Microsoft.Extensions.Configuration;
using Broadcast.Prep.Client;

Console.WriteLine("Starting");

IConfigurationRoot? configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Settings settings = new();
configuration.Bind("Settings", settings);

var serviceDate = DateOnly.FromDateTime(DateTime.Today);
while (serviceDate.DayOfWeek != DayOfWeek.Sunday)
    serviceDate = serviceDate.AddDays(1);

var targetFileName = $"{serviceDate:yyyy-MM-dd} FINAL.pages";
var targetFileFullPath = $"{settings.PagesSourceFolder}\\{targetFileName}";

Console.WriteLine($"Looking for {targetFileName}");

var targetFile = new FileInfo(targetFileFullPath);

if(!targetFile.Exists)
{
    Console.WriteLine($"{targetFileFullPath} not found!");
    Console.ReadKey();
    Environment.Exit(1);
}

Console.WriteLine($"{targetFileFullPath} found.");

targetFile.CopyTo($"{settings.PagesDestinationFolder}\\Current.pages", true);

// write date to date txt file
if (File.Exists(settings.DateTxtPath))
    File.Delete(settings.DateTxtPath);
File.WriteAllText(settings.DateTxtPath, serviceDate.ToLongDateString());

// write title and description file
if (File.Exists(settings.TitleAndDescriptionTxtPath))
    File.Delete(settings.TitleAndDescriptionTxtPath);

var titleAndDescription = settings.TitleAndDescriptionTemplate
    .Replace("{ServiceDate}", serviceDate.ToString("M/d/yyyy"));

File.WriteAllText(settings.TitleAndDescriptionTxtPath, titleAndDescription);


Console.WriteLine("Done. Press any key to exit.");
Console.ReadKey();
Environment.Exit(0);