import { createServer } from 'http'
import { parse } from 'url'
import next from 'next'
import fs from 'fs'
import mime from 'mime'
 
const port = parseInt(process.env.PORT || '3000', 10)
const dev = true
const app = next({ dev })
const handle = app.getRequestHandler()
 
app.prepare().then(() => {
  createServer((req, res) => {
    const parsedUrl = parse(req.url, true)
    const { pathname } = parsedUrl
    if (pathname.startsWith("/snippets/")) {
      const parts = req.url.split('/')
      const fPath = parts.slice(1, 3).join('/') + "/wwwroot/" + parts.slice(3).join('/')
      fs.readFile(fPath, (err, data) => {
        if (err) {
          res.statusCode = 404
          res.end('Snippet not found')
          return
        }
        // Set content-type based on file extension if needed
        res.statusCode = 200
        const mimeType = mime.getType(fPath) || 'application/octet-stream';
        res.setHeader('Content-Type', mimeType)
        res.end(data)
      })
    }
    else {
      handle(req, res, parsedUrl)
    }
  }).listen(port)
 
  console.log(
    `> Server listening at http://localhost:${port} as ${
      dev ? 'development' : process.env.NODE_ENV
    }`
  )
})