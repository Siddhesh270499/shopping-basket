# Shopping Basket – Angular front end

Angular 21 app (standalone components, signals, Vitest). It calls the ASP.NET Core API; all prices are worked out on the server.

```bash
npm install
npm start          # http://localhost:4200, /api is proxied to http://localhost:5000 (proxy.conf.json)
npm test -- --watch=false
npm run build      # builds into ../src/ShoppingBasket.Api/wwwroot so the API serves the UI itself
```

See the main README one folder up for the design.
