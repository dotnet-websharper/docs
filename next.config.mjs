import { createMDX } from 'fumadocs-mdx/next';

const basePathToUse = process.env.GHREPO !== undefined ? "/" + process.env.GHREPO : "";
console.log("BASEPATH:", basePathToUse);
const withMDX = createMDX();

/** @type {import('next').NextConfig} */
const config = {
  reactStrictMode: true,
  output: 'export',
  assetPrefix: '',
  basePath: basePathToUse
};


export default withMDX(config);
