# Build
`dotnet build`

# Run
`dotnet run --project ./DT2SG_app/DT2SG_app.fsproj`
<!-- 
# Run demo
`cd ./DT2SG_app/clear_demo.sh`
`dotnet run --project ./DT2SG_app/DT2SG_app.fsproj -root ./demo/source -metadata ./demo/source_metadata/metadata_example.csv -ignore ./demo/source_metadata/ignore_example.csv --committer_name "Pippo" -committer_email pippo@example.com`
 -->
# Release
`dotnet publish -c Release -r ubuntu.18.04-x64`
`--self-contained true`
or

`dotnet publish -c Release -r win10-x64`
`--self-contained true`
