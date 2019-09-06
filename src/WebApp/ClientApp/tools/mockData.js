const accounts = [
  {
    id: 1,
    title: "Securing React Apps with Auth0"
  },
  {
    id: 2,
    title: "React: The Big Picture"
  },
  {
    id: 3,
    title: "Creating Reusable React Components"
  },
  {
    id: 4,
    title: "Building a JavaScript Development Environment"
  },
  {
    id: 5,
    title: "Building Applications with React and Redux"
  },
  {
    id: 6,
    title: "Building Applications in React and Flux"
  },
  {
    id: 7,
    title: "Clean Code: Writing Code for Humans"
  },
  {
    id: 8,
    title: "Architecting Applications for the Real World"
  },
  {
    id: 9,
    title: "Becoming an Outlier: Reprogramming the Developer Mind"
  },
  {
    id: 10,
    title: "Web Component Fundamentals"
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
