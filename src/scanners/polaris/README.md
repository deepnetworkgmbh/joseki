# Polaris scanner

## Test data

The scanner is able to run in _fake_ mode, when it sends to _exporter_ (e.g. Blob Storage) pre-generated data for requested dates.

`polaris_test_data.tar.xz` contains pre-generated test cluster scans. These data could be used in conjunction with fake-scanner config `fake-scanner-config.yaml`:

1. extract archive with `tar -xf polaris_test_data.tar.xz -C ./examples`;
2. insert actual `from` and `to`, `storageBaseUrl` and `sasToken` values to `fake-scanner-config.yaml`;
3. run the scanner. For example: `go run .`
