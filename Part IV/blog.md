# ASP.NET Core中使用GraphQL - 第四章 GraphiQL

![](images\banner-8-1100x550.jpg)



ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入

------

<code>GraphiQL</code>是一款内置在浏览器中的<code>GraphQL</code>探索工具，这个是开发<code>GraphQL</code>的一个必备工具。它就相当于WebApi中的Swagger, 使用这个工具就可以看到你的<code>GraphQL中</code>所有配置的结构，并可以在浏览器中测试你的<code>query</code>, <code>mutation</code>

> 现在除了<code>GraphiQL</code>, <code>graphql-dotnet</code>还提供了另外一个[GraphQL.Server](GraphQL for .NET - Subscription Transport WebSockets)的类库, 它也可以生成一个界面更优雅的探索工具，但是由于作者声明了还未在重型生产环境中测试过，所以这里先不做介绍，后续我会单独写一篇博文来介绍一下。

要想使用<code>GraphiQL</code>, 我们需要借助NodeJs中的npm和webpack.

首先我们在当前Hello World项目中创建一个package.json文件, 内容如下

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
然后可以使用一下命令，安装package.json中所有预定义的库

```
npm install
```

下一步，我们需要在当前项目目录中创建一个新的文件夹<code>ClientApp</code>, 并在其中添加2个文件<code>app.js</code>和<code>app.css</code>, 其文件内容如下。

##### app.js

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

##### app.css

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

<code>GraphiQL</code>是一个客户端库，它提供了一个<code>React</code>组件<code><GraphiQL /></code>, 这个组件用来呈现整个用户界面。这个组件有一个<code>fetcher</code>属性， 这个属性可以附加一个<code>function</code>。 附加的<code>function</code>返回了一个HTTP promise对象，它仅仅是模仿了我们在Postman中测试的POST请求。所以这些设置都写在<code>app.js</code>文件中。

下一步我们需要在<code>wwwroot</code>目录中添加一个<code>index.html</code>, 这里我们会将<code><GraphiQL /></code>组件的内容呈现在<code>id="app"</code>的<code>div</code>中.

##### index.html

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

在<code>index.html</code>文件中，我们引入了一个<code>bundle.js</code>和一个<code>style.css</code>文件。这2个文件是通过脚本编译出来的，所以这里我们需要添加一个<code>webpack.config.js</code>

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

最后我们还需要添加一个.babelrc文件，其内容如下

##### .babelrc

```json
{
  "presets": ["env", "react"]
}
```

以上文件添加完成之后，我们可以在在命令行使用<code>webpack</code>命令编译生成这2个文件。
```
C:\chapter4>webpack
Hash: e8082714ec56e818e1f4
Version: webpack 3.12.0
Child
    Hash: e8082714ec56e818e1f4
    Time: 6645ms
        Asset     Size  Chunks                    Chunk Names
    bundle.js  2.76 MB       0  [emitted]  [big]  bundle
    style.css  39.7 kB       0  [emitted]         bundle
      [33] (webpack)/buildin/global.js 509 bytes {0} [built]
     [128] ./node_modules/graphql-language-service-interface/dist ^.*$ 807 bytes {0} [built]
     [137] ./ClientApp/app.js 996 bytes {0} [built]
     [234] (webpack)/buildin/module.js 517 bytes {0} [built]
     [292] ./ClientApp/app.css 41 bytes {0} [built]
     [297] ./node_modules/css-loader!./ClientApp/app.css 301 bytes [built]
        + 292 hidden modules
    Child extract-text-webpack-plugin node_modules/extract-text-webpack-plugin/dist node_modules/css-loader/index.js!node_modules/graphiql/graphiql.css:
           2 modules
    Child extract-text-webpack-plugin node_modules/extract-text-webpack-plugin/dist node_modules/css-loader/index.js!ClientApp/app.css:
           [0] ./node_modules/css-loader!./ClientApp/app.css 301 bytes {0} [built]
            + 1 hidden module

C:\chapter4>
```

在服务器端，我们需要修改<code>Startup.cs</code>文件，在<code>Configure</code>方法中添加静态文件中间件和默认页中间件，修改后最终的<code>Configure</code>方法代码如下

```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)  
{
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseMiddleware<GraphQLMiddleware>();
}
```

现在我们启动项目，你将会看到如下图所示的用户界面。

![1541592852081](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\1541592852081.png)

在右侧的Documentation Explorer面板中，你可以看到定义的所有<code>query</code>, 并且可以了解到哪些字段是可用的，以及它们是干什么用的。

<code>GraphiQL</code>提供了许多很棒的功能

- 语法高亮
- 编写<code>GraphQL</code>查询时，字段，参数，类型等的自动感知
- 实时错误高亮以及报告
- 自动补全查询
- 可以在浏览器中模拟请求， 运行检查查询结果