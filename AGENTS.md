# Agent guidelines

## Testing

- For non-code changes (menu items, copy/text, links, small styling tweaks, asset
  swaps), make the edit and stop. Do **not** run the production build, restart the
  dev server, or do route smoke tests.
- Run the full verification (production build, dev-server check, route checks) only
  when the change could plausibly break something — e.g. routing, layouts, build
  config, dependencies, or actual code.
