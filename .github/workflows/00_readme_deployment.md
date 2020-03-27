# Deployment

Most of `deploy-to-k8s` steps loads config files from Github secrets. This step expects content to be encoded as base64 without line wrapping. To generate such string run: `base64 -w 0 config.yaml > encoded` and then insert `encoded` content to github secret.
