<template>
  <form>
    <h1>XDownloader</h1>
    <div class="grid">
      <div>
        <input type="text" placeholder="Get_Links_From_Protector" v-model="protectorLink">
        <button @click="getLinksFromProtector">Valider</button>
        <span v-if="protectorLinkUploading">Requête en cours</span>
        <span v-if="protectorLinkUploadDone">Requête terminée ! (le téléchargement est en cours en arrière plan)</span>
      </div>
      <div>
        <input type="text" placeholder="Get_Link_From_Source" v-model="sourceLink">
        <button @click="getLinkFromSource" disabled>Valider</button>
      </div>
    </div>
  </form>
</template>

<script>
  export default {
    name    : "Download",
    computed: {},
    data    : function () {
      return {
        protectorLink          : "",
        protectorLinkUploading : false,
        protectorLinkUploadDone: false,
        sourceLink             : "",
      };
    },
    methods : {
      getLinksFromProtector() {
        const xhr = new XMLHttpRequest();
        xhr.open("POST", "http://localhost:5000/api/LinksFromProtector");

        xhr.setRequestHeader("Content-Type", "application/json");
        // Create request
        const linksFromProtector = {
          hosts: [],
          url  : "https://www.dl-protect1.com/1234556001234556021234556101234556156bsfkx1psaa8",
        };

        xhr.send(JSON.stringify(linksFromProtector));

        xhr.onreadystatechange = function () {
          //Call a function when the state changes.
          if (xhr.readyState === XMLHttpRequest.DONE && xhr.status === 200) {
            this.protectorLink          = "";
            this.protectorLinkUploading = true;
          }
        };
      },
      getLinkFromSource() {},
    },
  };
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
