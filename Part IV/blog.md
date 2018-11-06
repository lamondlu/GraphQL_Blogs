# ASP.NET Core中使用GraphQL - 第四章 GraphiQL

![](C:\Users\Administrator\OneDrive\博客\GraphQL\Part III\images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入

------

## 



```json
{
  "name": "GraphQLAPI",
  "version": "1.0.0",
  "main": "index.js",
  "author": "Fiyaz Hasan",
  "license": "MIT",
  "dependencies": {
    "graphiql": "^0.11.11",
    "graphql": "^0.13.2",
    "isomorphic-fetch": "^2.2.1",
    "react": "^16.3.1",
    "react-dom": "^16.2.0"
  },
  "devDependencies": {
    "babel-cli": "^6.26.0",
    "babel-loader": "^7.1.4",
    "babel-preset-env": "^1.6.1",
    "babel-preset-react": "^6.24.1",
    "css-loader": "^0.28.11",
    "extract-text-webpack-plugin": "^3.0.2",
    "ignore-loader": "^0.1.2",
    "style-loader": "^0.20.3",
    "webpack": "^3.11.0"
  }
}
```

```
npm install
```

app.js

```javascript
import React from 'react';  
import ReactDOM from 'react-dom';  
import GraphiQL from 'graphiql';  
import fetch from 'isomorphic-fetch';  
import 'graphiql/graphiql.css';  
import './app.css';

function graphQLFetcher(graphQLParams) {  
  return fetch(window.location.origin + '/api/graphql', {
    method: 'post',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(graphQLParams)
  }).then(response => response.json());
}

ReactDOM.render(  
  <GraphiQL fetcher={graphQLFetcher} />,
  document.getElementById('app')
);
```

app.css

```css
html, body {  
    height: 100%;
    margin: 0;
    overflow: hidden;
    width: 100%
}

#app {
    height: 100vh
}
```

index.html

```html
<!DOCTYPE html>  
<html>  
<head>  
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width" />
    <title>GraphiQL</title>
    <link rel="stylesheet" href="/style.css" />
</head>  
<body>  
    <div id="app"></div>
    <script src="/bundle.js" type="text/javascript"></script>
</body>  
</html>  
```

##### webpack.config.js

```javascript
const webpack = require('webpack');  
var path = require('path');  
const ExtractTextPlugin = require('extract-text-webpack-plugin');

module.exports = [  
    {
        entry: {
            'bundle': './ClientApp/app.js',
        },

        output: {
            path: path.resolve('./wwwroot'),
            filename: '[name].js'
        },

        resolve: {
            extensions: ['.js', '.json']
        },

        module: {
            rules: [
                { test: /\.js/, use: [{
                    loader: 'babel-loader'
                }], exclude: /node_modules/ },
                {
                    test: /\.css$/, use: ExtractTextPlugin.extract({
                        fallback: "style-loader",
                        use: "css-loader"
                    })
                },
                { test: /\.flow/, use: [{
                    loader: 'ignore-loader'
                }] }
            ]
        },

        plugins: [
            new ExtractTextPlugin('style.css', { allChunks: true })
        ]
    }
];
```

##### .babelrc

```
{
  "presets": ["env", "react"]
}
```

```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)  
{
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseMiddleware<GraphQLMiddleware>();
}
```

