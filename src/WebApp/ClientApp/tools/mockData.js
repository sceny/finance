const accounts = [
    {
      id: 1,
      title: "RBC 3132",
      slug: "rbc-3132",
      institutionId: 1
    },
    {
      id: 2,
      title: "RBC 5445",
      slug: "rbc-5445",
      institutionId: 1
    },
    {
      id: 3,
      title: "CapitalOne xxx9999",
      slug: "CapitalOne xxx9999",
      institutionId: 2
    }
];

const institutions = [
    {
      id: 1,
      title: "RBC",
      slug: "rbc"
    },
    {
        id: 2,
        title: "CapitalOne",
        slug: "capitalone"
    }
];

const newAccount = {
  id: null,
  title: ""
};

// Using CommonJS style export so we can consume via Node (without using Babel-node)
module.exports = {
  newCourse: newAccount,
  accounts: accounts,
  institutions: institutions
};
