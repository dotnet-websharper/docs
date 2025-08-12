module.exports = {
  root: "wwwroot",
  build: {
    rollupOptions: {
      input: [
        "./Scripts/MessagePassingWorker.min.js"
      ]
    }
  }
}