<template>
  <form>
    <h1>XDownloader</h1>
    <div class="grid">
      <div>
        <input type="text" placeholder="Get_Link_From_Protector_Page" v-model="protectorLink">
        <button @click="getLinkFromProtectorPage">Valider</button>
      </div>
      <div></div>
    </div>
  </form>
</template>

<script lang="ts">
  import Vue from "vue";

  export default Vue.extend( {
    name: "Download",
    data: {
      protectorLink: "",
    },
    methods: {
      getLinkFromProtectorPage(): void {
        const xhr = new XMLHttpRequest();
        xhr.open("POST", "http://localhost:")
      }
    }
  } );
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
  h3 {
    margin: 40px 0 0;
  }

  a {
    color: #42b983;
  }

  .grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
  }
</style>
