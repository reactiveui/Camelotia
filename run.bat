echo "Starting app..."
dotnet clean
dotnet restore
dotnet build
cd Camelotia.Presentation.Avalonia
dotnet run --framework netcoreapp2.0