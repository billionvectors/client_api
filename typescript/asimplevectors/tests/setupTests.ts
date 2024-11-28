process.on("unhandledRejection", (reason) => {
    console.error("Unhandled Rejection:", reason);
    throw reason;
  });
  