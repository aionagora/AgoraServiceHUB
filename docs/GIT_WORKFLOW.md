# Git Workflow Documentation

This document outlines the Git workflow used in the project, including the branching strategy, merge strategies, versioning, and examples.

## Branching Strategy

The following branches are used in the project:

- **main**: The main branch represents the production code. Only stable releases are merged into this branch.
- **develop**: This branch serves as the integration branch for features. It contains the latest development changes that are intended for the next release.
- **feature/**: Feature branches are created off the `develop` branch. These branches are used to implement new features or enhancements. Naming convention: `feature/<feature-name>`.
- **release/**: Release branches are created off the `develop` branch when preparing for a new release. These branches allow for last-minute changes and preparation before merging into `main`. Naming convention: `release/<version>`.
- **hotfix/**: Hotfix branches are created off the `main` branch to address critical bugs in the production code. After fixing the bug, hotfix branches are merged back into both `main` and `develop`. Naming convention: `hotfix/<issue>`.

## Branch Rules

1. Only the repository administrator can merge into the `main` branch.
2. Feature branches must be reviewed and approved before merging into `develop`.
3. Each release branch must go through quality assurance before merging into `main`.
4. Hotfix branches must have immediate attention and cannot be delayed.

## Merge Strategies

- **Merge Commit**: The default strategy that preserves the history of changes and creates a commit for merging branches.
- **Squash**: Combines all feature branch commits into a single commit when merging, keeping the history clean.
- **Rebase**: Reapplies commits from the feature branch on top of the base branch, maintaining a linear commit history.

## Versioning

We follow Semantic Versioning (SemVer) for versioning our releases:

- **MAJOR version** when there are incompatible API changes,
- **MINOR version** when functionality is added in a backward-compatible manner,
- **PATCH version** when backward-compatible bug fixes are introduced.

### Examples

1. **Creating a Feature Branch**:
   ```bash
   git checkout develop
   git checkout -b feature/new-feature
   ```

2. **Merging a Feature Branch**:
   ```bash
   git checkout develop
   git merge feature/new-feature
   ```

3. **Creating a Release Branch**:
   ```bash
   git checkout develop
   git checkout -b release/1.0.0
   ```

4. **Creating a Hotfix Branch**:
   ```bash
   git checkout main
   git checkout -b hotfix/issue
   ```
   
5. **Merging a Hotfix**:
   ```bash
   git checkout main
   git merge hotfix/issue
   git checkout develop
   git merge hotfix/issue
   ```

This workflow ensures that we maintain a clean and organized project while allowing for efficient development and release cycles.