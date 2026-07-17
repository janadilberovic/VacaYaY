/** @type {import('next').NextConfig} */

// The API ships no CORS headers, so we proxy same-origin /api requests through Next
// to Kestrel. Default targets the http profile `dotnet run` uses; override with
// API_TARGET (e.g. https://localhost:7273 for the https profile — the dev script sets
// NODE_TLS_REJECT_UNAUTHORIZED=0 so the rewrite accepts the self-signed dev cert).
const API_TARGET = process.env.API_TARGET ?? 'http://localhost:5266'

const nextConfig = {
  async rewrites() {
    return [{ source: '/api/:path*', destination: `${API_TARGET}/api/:path*` }]
  },
}

export default nextConfig
