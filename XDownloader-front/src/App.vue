<template>
    <div id="app">
        <div id="nav">
            <router-link to="/">Home</router-link>
            |
            <router-link to="/about">About</router-link>
        </div>

        <div id="left"></div>

        <div id="content">
            <router-view/>
        </div>

        <div id="right"></div>
    </div>
</template>

<style>
#app {
  font-family: 'Fira Sans', sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  display: grid;
  grid-template-areas:
    "header header header header header"
    "left content content content right"
    "left footer footer footer footer";
  grid-gap: 1em;
}

#nav {
  padding: 30px;
  grid-area: header;
}

#nav a {
  font-weight: bold;
  color: #2c3e50;
}

#nav a.router-link-exact-active {
  color: #42b983;
}

#left {
  grid-area: left;
}

#content {
  grid-area: content;
  justify-self: center;
}
</style>
