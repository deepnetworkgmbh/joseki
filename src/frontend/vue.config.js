module.exports = {
  devServer: {
    proxy: {
      "/api|/image": {
        target: "http://localhost:8888",
        ws: true,
        changeOrigin: true
      }
    }
  }
};
