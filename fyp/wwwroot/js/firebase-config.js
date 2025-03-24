// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
import { getAuth } from "https://www.gstatic.com/firebasejs/9.6.10/firebase-auth.js";
import { getFirestore } from "https://www.gstatic.com/firebasejs/9.6.10/firebase-firestore.js";

const firebaseConfig = {
    apiKey: "AIzaSyB18pzPJZgonnb28oHW7wQQbOQwGJeky4k",
    authDomain: "inventoryfyp-6d9a0.firebaseapp.com",
    projectId: "inventoryfyp-6d9a0",
    storageBucket: "inventoryfyp-6d9a0.appspot.com",
    messagingSenderId: "309897826777",
    appId: "1:309897826777:web:691a93a7b76f3fa03649c8",
    measurementId: "G-BERC0TGQ91"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);
const db = getFirestore(app);

export { auth, db };


