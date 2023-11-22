const fs = require('fs');

const copyDirRecursiveSync = (source, target) => {
  if (!fs.existsSync(target)) {
    fs.mkdirSync(target);
  }

  fs.readdirSync(source).forEach((file) => {
    const sourcePath = `${source}/${file}`;
    const targetPath = `${target}/${file}`;

    if (fs.lstatSync(sourcePath).isDirectory()) {
      copyDirRecursiveSync(sourcePath, targetPath);
    } else {
      fs.copyFileSync(sourcePath, targetPath);
    }
  });
};

copyDirRecursiveSync('node_modules/reflect-metadata', 'node_modules/angular2-universal-polyfills/node_modules/reflect-metadata');
