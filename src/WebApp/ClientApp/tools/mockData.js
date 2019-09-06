const accounts = [
    {
      id: 1,
      title: "Securing React Apps with Auth0",
      slug: "react-auth0-authentication-security"
    },
    {
      id: 2,
      title: "React: The Big Picture",
      slug: "react-big-picture"
    },
    {
      id: 3,
      title: "Creating Reusable React Components",
      slug: "react-creating-reusable-components"
    },
    {
      id: 4,
      title: "Building a JavaScript Development Environment",
      slug: "javascript-development-environment"
    },
    {
      id: 5,
      title: "Building Applications with React and Redux",
      slug: "react-redux-react-router-es6"
    },
    {
      id: 6,
      title: "Building Applications in React and Flux",
      slug: "react-flux-building-applications"
    },
    {
      id: 7,
      title: "Clean Code: Writing Code for Humans",
      slug: "writing-clean-code-humans"
    },
    {
      id: 8,
      title: "Architecting Applications for the Real World",
      slug: "architecting-applications-dotnet"
    },
    {
      id: 9,
      title: "Becoming an Outlier: Reprogramming the Developer Mind",
      slug: "career-reboot-for-developer-mind"
    },
    {
      id: 10,
      title: "Web Component Fundamentals",
      slug: "web-components-shadow-dom"
    }
];

const newAccount = {
  id: null,
  title: ""
};

// Using CommonJS style export so we can consume via Node (without using Babel-node)
module.exports = {
  newCourse: newAccount,
  accounts: accounts
};
