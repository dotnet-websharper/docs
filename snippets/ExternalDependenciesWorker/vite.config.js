module.exports = {
  root: "wwwroot",
  build: {
    rollupOptions: {
      input: [
        "./Scripts/ExternalDependenciesWorker.min.js"
      ]
    }
  }
}