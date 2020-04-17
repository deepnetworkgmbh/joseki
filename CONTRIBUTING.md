# Contribution Guide

The development process is optimized for a small team, and is focused on keeping sources at deployable state. Therefore, everything is stored in a single repository and pushes to `master` branch are permitted only through pull-requests, protected with ci-pipelines.

When project grows - the repository could be split according to service boundaries.

## Single repository

All the sources, documentation, getting-started samples are stored in the single repository:

- `.github` folder consists of pr-templates, issue-templates, github-actions pipelines.
- `src` folder has four subfolders:
  - `backend`: web-api, data management, reporting, etc,
  - `frontend`: web-application,
  - `deploy`: templates/scripts to provision required infrastructure in misc clouds,
  - `scanners` has a dedicated sub-folder for each scanner type.
- `root` has project-level documents: [readme](/README.md), [product overview](/PRODUCTOVERVIEW.md), [tech-design](./TECH_DESIGN.md), contributor guidelines (this), and others

As different services might use different technology-sets, each service folder has own readme doc with getting-started info. For example, [backend readme](/src/backend/README.md).

## Ready to run at any commit in master

Any commit in `master` branch should be in fully-functional state. To ensure this:

- the branch should be protected against direct pushes;
- all the changes should go through pull-requests;
- each PR should be validated with ci-pipeline.

## CI/CD

The repository has a set of continuous-integration pipelines to protect `master` branch from unsafe code and release trusted docker-images.

Logically, pipelines can be grouped into two categories:

1. _Pull Request_. They validate that code is safe to be merged: compile; run tests/analyzers/linters. Each service has a separate pipeline.
2. _Commit to `master`_. These pipelines main task is to publish ready-to-use docker images. Each service has a separate pipeline.

## Adding new scanners types, Blob Storages, Messaging Services, Databases

If you want to add any other **Message Queue** or **Blob Storage** implementation, add new scanner type - please create a *github issue* or vote for the existing one. Also *pull requests* are more than welcome ;)
