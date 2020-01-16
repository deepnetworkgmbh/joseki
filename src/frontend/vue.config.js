module.exports = {
  devServer: {
    proxy: {
      "/api": {
        target: "http://localhost:8888",
        ws: true,
        changeOrigin: true
      }
    }
  }
};
