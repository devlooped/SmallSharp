#:property ImplicitUsings true
#:property Nullable enable
#:package Humanizer@2.14.1

using Humanizer;

var dotNet9Released = DateTimeOffset.Parse("2024-12-03");
var since = DateTimeOffset.Now - dotNet9Released;

Console.WriteLine($"It has been {since.Humanize()} since .NET 9 was released.");