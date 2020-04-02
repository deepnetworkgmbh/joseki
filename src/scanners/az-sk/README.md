# Azure Security Kit scanner

## Test data

The scanner is able to run in _fake_ mode, when it sends to _exporter_ (e.g. Blob Storage) pre-generated data for requested dates.

`azsk_test_data.tar.xz` contains pre-generated test subscription scans. These data could be used in conjunction with fake-scanner config `fake-config-sample.az.yaml`:

1. extract archive with `tar -xf azsk_test_data.tar.xz ./azsk_test_data`;
2. insert actual `fakeResultsFolderPath`, `basePath` and `sas` values to `fake-config-sample.az.yaml`;
3. run the scanner. For example: `cli.dll -c PATH_TO_FAKE_CONFIG -s 5720db52-2fb2-4568-bb63-20c7c0dd0a3e --from 2020-03-01 --to 2020-03-31`, where
   - `-c` is absolute path to  config created at step two,
   - `-s` is subscription-id (note, `5720db52-2fb2-4568-bb63-20c7c0dd0a3e` was used in test-data files),
   - `--from` and `--to` range of dates to generate test-data; these properties are optional; when they are skipped scanner generates only a single scan result for current date.

**NOTE:** by default, scanner requires two environment variables: `SCANNER_VERSION` and `AZSK_VERSION`. To set all the cli parameters and env-vars in a single place, you can use `dotnet run --launch-profile launchSettings.json /joseki/src/scanners/az-sk/src/cli` command, where `launchSettings.json` is config with pre-defined properties, like in `/joseki/src/scanners/az-sk/src/cli/Properties/launchSettings.json`
