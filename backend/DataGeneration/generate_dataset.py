import json
import random
import csv

doms = ['frontend', 'backend', 'fullstack', 'data', 'devops', 'ml', 'business', 'design', 'healthcare', 'education']

# 150 highly proper questions, each with tailored option text for all 10 domains.
# The options directly answer the question prompt in a way that matches each domain's mindset and vocabulary.
quiz_questions_source = [
    # 1. Technical Focus and Development Tasks (1-10)
    {
        "text": "When building a web application, what is your primary focus?",
        "options": {
            "frontend": "Designing responsive page layouts, handling UI state, and creating clean interactions.",
            "backend": "Architecting secure API routes, managing databases, and processing business logic.",
            "fullstack": "Connecting front-end components with server logic and managing end-to-end features.",
            "data": "Structuring data storage, building analytics pipelines, and running complex queries.",
            "devops": "Setting up build automation, managing servers, and maintaining continuous delivery.",
            "ml": "Evaluating model performance, processing neural features, and training predictions.",
            "business": "Conducting user research, assessing ROI, and setting product requirements.",
            "design": "Designing high-fidelity prototypes, choosing color schemes, and planning user flows.",
            "healthcare": "Ensuring strict patient privacy, system safety, and compliance with medical standards.",
            "education": "Structuring simple navigation, creating clear instructions, and ensuring learning accessibility."
        }
    },
    {
        "text": "What type of problem are you most comfortable solving in your daily work?",
        "options": {
            "frontend": "Fixing styling inconsistencies, rendering arrays of data, and fixing UI lag.",
            "backend": "Handling database deadlocks, slow API endpoints, and server authentication errors.",
            "fullstack": "Debugging data-sync issues between local UI stores and backend database records.",
            "data": "Fixing broken ETL pipelines, cleaning messy CSV columns, and optimizing aggregate queries.",
            "devops": "Resolving failed container builds, misconfigured network routes, and high CPU usage alerts.",
            "ml": "Tuning hyper-parameters, resolving model overfitting, and handling biased training data.",
            "business": "Addressing declining user retention, poor conversion rates, and budget constraints.",
            "design": "Resolving confusing navigational layouts, low readability contrast, and poor accessibility.",
            "healthcare": "Diagnosing digital health record mismatches and maintaining system uptime for medical staff.",
            "education": "Fixing low course completion rates and designing simple materials for struggling learners."
        }
    },
    {
        "text": "Which software development tool or technology do you use or study the most?",
        "options": {
            "frontend": "Modern JS libraries and CSS processors like React, Vue, Tailwind, or Next.js.",
            "backend": "Server frameworks and databases like Node.js, C#, PostgreSQL, or Redis.",
            "fullstack": "Integrated systems like Next.js, full-stack frameworks, and monolithic web packages.",
            "data": "Data processing engines and warehouses like SQL, Apache Spark, Snowflake, or Python Pandas.",
            "devops": "Infrastructure automation tools like Docker, Kubernetes, Terraform, or GitHub Actions.",
            "ml": "Machine learning libraries and notebooks like PyTorch, TensorFlow, or Jupyter.",
            "business": "Analytics platforms and task management software like Jira, Tableau, or Mixpanel.",
            "design": "Visual mockup and vector tools like Figma, Sketch, Adobe Illustrator, or Miro.",
            "healthcare": "Electronic Health Record (EHR) integrations and FHIR medical exchange protocols.",
            "education": "Learning Management Systems (LMS) like Canvas, Moodle, and digital curriculum software."
        }
    },
    {
        "text": "What would make you feel most proud at the completion of a major project?",
        "options": {
            "frontend": "A pixel-perfect, highly responsive interface that loads within milliseconds.",
            "backend": "A highly scalable API architecture capable of processing millions of requests without error.",
            "fullstack": "A fully functioning product built entirely from scratch, covering all technical layers.",
            "data": "An interactive dashboard revealing previously hidden business trends and metrics.",
            "devops": "A stable deployment environment with automated fallback recovery and zero downtime.",
            "ml": "An accurate model that generalizes perfectly to real-world test scenarios.",
            "business": "A successful product launch that hits all user growth and revenue targets.",
            "design": "A design system adopted by multiple teams, creating a unified product experience.",
            "healthcare": "A secure system that ensures patients receive medical care quickly and safely.",
            "education": "A learning platform that helps students master complex concepts successfully."
        }
    },
    {
        "text": "Which of these tasks would you choose to perform if you had a free afternoon?",
        "options": {
            "frontend": "Experimenting with a new interactive animation library or component style.",
            "backend": "Refactoring database queries to improve execution plans and server response times.",
            "fullstack": "Building a simple utility app that integrates an external service from start to finish.",
            "data": "Analyzing a public dataset to find correlations and visualizing the results.",
            "devops": "Setting up automated Slack alerts for server resource warnings and health status.",
            "ml": "Fine-tuning a neural model on a new dataset to check prediction accuracy.",
            "business": "Mapping out the product roadmap for the next quarter and writing feature specs.",
            "design": "Conducting user tests, drawing vector icons, and polishing UI components.",
            "healthcare": "Reviewing medical compliance guidelines to ensure app privacy standards are met.",
            "education": "Writing a detailed step-by-step tutorial or planning a training workshop."
        }
    },
    {
        "text": "How do you prefer to approach the design of system components?",
        "options": {
            "frontend": "Breaking down screens into modular, reusable UI components with clear props.",
            "backend": "Structuring logic into microservices, controller classes, and data access layers.",
            "fullstack": "Planning how visual UI cards map to database models and server schemas.",
            "data": "Designing star-schema databases, staging areas, and clean data warehouse models.",
            "devops": "Defining infrastructure in configuration files to maintain server environments automatically.",
            "ml": "Architecting neural layers, training pipelines, and validation splits.",
            "business": "Defining product requirements, user personas, and target customer outcomes.",
            "design": "Creating consistent layout grids, design systems, and wireframe prototypes.",
            "healthcare": "Ensuring clinical workflows and medical patient data models match standard guidelines.",
            "education": "Structuring content into sequential modules, lessons, and student test checkpoints."
        }
    },
    {
        "text": "What is your main strategy to improve the security of an application?",
        "options": {
            "frontend": "Sanitizing user inputs to prevent XSS attacks and securing token cookies.",
            "backend": "Implementing secure password hashing, JWT validations, and rate-limiting APIs.",
            "fullstack": "Validating credentials at both the client-side forms and server-side logic layers.",
            "data": "Anonymizing sensitive data rows and managing database access permissions.",
            "devops": "Configuring network firewalls, securing SSH keys, and managing private VPC subnets.",
            "ml": "Securing API model endpoints and checking training sets for poisoned inputs.",
            "business": "Conducting risk assessments, ensuring compliance audits, and acquiring insurance.",
            "design": "Designing clear permission notices and helping users understand security settings.",
            "healthcare": "Applying strict HIPAA/medical compliance encryption to all clinical data records.",
            "education": "Ensuring student privacy compliance (like COPPA/FERPA) and safe digital access."
        }
    },
    {
        "text": "When a web page loads slowly, what do you look at first?",
        "options": {
            "frontend": "The browser console, JS bundle sizes, and image render dimensions.",
            "backend": "Database query times, server-side execution latency, and API caching headers.",
            "fullstack": "The network tab to inspect HTTP request logs and full-stack response times.",
            "data": "The database index status and execution plans for analytical queries.",
            "devops": "The load balancer status, network bandwidth limits, and CPU usage metrics.",
            "ml": "The inference latency of the prediction service and GPU server load.",
            "business": "The bounce rate increase and potential conversion losses from slow load times.",
            "design": "Visual layout shifts, large graphic files, and non-optimized web font configurations.",
            "healthcare": "The availability of critical patient record charts on slow clinical interfaces.",
            "education": "The accessibility of lesson files for students in low-bandwidth regions."
        }
    },
    {
        "text": "How do you prefer to collaborate with other members of your team?",
        "options": {
            "frontend": "Working with designers to bring wireframes to life and writing reusable UI widgets.",
            "backend": "Providing clean API definitions and documentation to client developers.",
            "fullstack": "Assisting both client-side and server-side developers to resolve integration conflicts.",
            "data": "Providing custom SQL views and data reports to marketing and business analysts.",
            "devops": "Configuring unified staging environments and helping coders debug deployment errors.",
            "ml": "Deploying model endpoints that developer teams can call from their applications.",
            "business": "Gathering feature feedback from sales, design, and developer leads.",
            "design": "Hosting design review sessions, taking user feedback, and building assets.",
            "healthcare": "Coordinating with medical doctors, nurses, and tech leads to safety-check software.",
            "education": "Coordinating with teachers, trainers, and content creators to plan materials."
        }
    },
    {
        "text": "What type of data do you prefer to work with in your projects?",
        "options": {
            "frontend": "JSON payloads, HTML tags, CSS variables, and DOM event states.",
            "backend": "Relational rows, transaction records, server settings, and routing maps.",
            "fullstack": "Unified application state, form inputs, and database schemas.",
            "data": "Large tables, timeseries logs, CSV files, and multi-dimensional matrices.",
            "devops": "Server metrics, YAML configs, deployment scripts, and resource limits.",
            "ml": "Image datasets, text arrays, token sequences, and probability matrices.",
            "business": "Revenue records, customer retention tables, market sizes, and survey ratings.",
            "design": "User session videos, design system variables, fonts, and graphical components.",
            "healthcare": "Electronic health charts, clinical lab reports, and medication histories.",
            "education": "Student test scores, assignment submissions, course ratings, and progress metrics."
        }
    },
    # 2. Dealing with Obstacles and Bugs (11-20)
    {
        "text": "If a user reports that a critical button is not working on a website, how do you debug it?",
        "options": {
            "frontend": "Inspecting DOM event listeners, browser console errors, and state mutations.",
            "backend": "Checking server logs, API request payloads, and database save status.",
            "fullstack": "Verifying the frontend request payload matching against server controller variables.",
            "data": "Running raw SQL commands to check if the button trigger updated database records.",
            "devops": "Checking API gateway logs and load-balancing router rules to ensure traffic flows.",
            "ml": "Checking if the button's AI recommendation output failed to return results.",
            "business": "Analyzing how many users fail to click the button and the business impact.",
            "design": "Checking if the button is visible, clickable, and visually clear to users.",
            "healthcare": "Ensuring the failure doesn't block clinical alerts or patient diagnosis screens.",
            "education": "Providing alternative task submission instructions for students while fixing the button."
        }
    },
    {
        "text": "When a server runs out of memory, what is your approach to resolve it?",
        "options": {
            "frontend": "Optimizing memory usage by cleaning client event listeners and image caches.",
            "backend": "Identifying backend memory leaks, closing database connections, and tuning caching.",
            "fullstack": "Reviewing client-server polling patterns and cleaning full-stack cache keys.",
            "data": "Breaking huge analytic queries into smaller batches to prevent DB server crashes.",
            "devops": "Configuring swap files, adjusting container memory limits, and setting up auto-scale.",
            "ml": "Reducing neural batch sizes and moving model weights from RAM to GPU memory.",
            "business": "Estimating the cost of upgrading server plans versus code optimization time.",
            "design": "Providing an elegant error page so users know the server is temporarily overloaded.",
            "healthcare": "Redirecting critical systems to backup medical servers to ensure patient safety.",
            "education": "Scaling up learning portal hosting to handle traffic during final exams."
        }
    },
    {
        "text": "If a database query takes too long, what is your direct fix?",
        "options": {
            "frontend": "Reducing the amount of data requested and lazy-loading non-essential components.",
            "backend": "Creating indexes, optimizing joins, and configuring query caching.",
            "fullstack": "Implementing pagination on the UI and optimizing backend controller queries.",
            "data": "Redesigning database schemas, using column stores, and rewriting SQL aggregates.",
            "devops": "Upgrading database server hardware, adjusting disk I/O, and adding read-replicas.",
            "ml": "Caching model inputs in rapid key-value stores to avoid repeated database searches.",
            "business": "Budgeting for better analytics software to improve database performance.",
            "design": "Designing skeleton loaders and informative progress bars to keep users engaged.",
            "healthcare": "Prioritizing critical medical chart queries over administrative operations.",
            "education": "Structuring student query calls to avoid system lag during class tests."
        }
    },
    {
        "text": "How do you handle ambiguous requirements from a project lead?",
        "options": {
            "frontend": "Creating quick visual mockups to confirm how pages should look and function.",
            "backend": "Writing unit tests and defining API contracts before writing server code.",
            "fullstack": "Building a basic functional MVP to show the end-to-end user flow.",
            "data": "Running basic descriptive analyses on the data to see what insights are possible.",
            "devops": "Setting up a standard staging environment to test configurations incrementally.",
            "ml": "Testing baseline models on sample datasets to define performance metrics.",
            "business": "Hosting a scoping meeting to draft detailed product requirement documents.",
            "design": "Conducting user surveys and drawing wireframes to clarify layout needs.",
            "healthcare": "Consulting health compliance officials to ensure requirements match safety laws.",
            "education": "Mapping out learning outcomes to align the lesson structure with user needs."
        }
    },
    {
        "text": "When code updates break existing features, what is your process?",
        "options": {
            "frontend": "Reviewing frontend component regression tests and checking browser behavior.",
            "backend": "Running server-side unit tests and checking API integrations.",
            "fullstack": "Running comprehensive integration tests across both UI and database layers.",
            "data": "Validating data integrity constraints and checking table relations.",
            "devops": "Reverting to the last stable container image deployment using git rollback.",
            "ml": "Comparing model evaluation metrics against the baseline version.",
            "business": "Assessing client dissatisfaction and setting up bug-fix prioritizations.",
            "design": "Checking if layout changes or UI updates confused returning users.",
            "healthcare": "Executing emergency rolls and verification to protect critical medical systems.",
            "education": "Notifying course tutors and students of technical issues with lesson pages."
        }
    },
    {
        "text": "What do you do when a user interface looks broken on mobile screens?",
        "options": {
            "frontend": "Adjusting CSS media queries, flexbox rules, and testing different breakpoints.",
            "backend": "Optimizing image compression APIs to serve lighter assets to mobile devices.",
            "fullstack": "Updating API payloads to send condensed data structures for mobile views.",
            "data": "Designing mobile-friendly reporting structures and summarized text cards.",
            "devops": "Configuring mobile-specific routing rules and CDN image optimization layers.",
            "ml": "Tuning light models that can perform inferences locally on mobile hardware.",
            "business": "Analyzing mobile web traffic metrics to determine mobile optimization budget.",
            "design": "Redesigning page layout grids, button sizes, and font scales for touch screens.",
            "healthcare": "Ensuring critical patient charts can be easily read on clinician smartphones.",
            "education": "Making sure video lessons and quizzes are fully responsive for student phones."
        }
    },
    {
        "text": "How do you handle a situation where data formatting is inconsistent?",
        "options": {
            "frontend": "Creating helper functions to clean and format strings before rendering them.",
            "backend": "Adding data validation middleware and API schema decorators.",
            "fullstack": "Implementing consistent parsing models in both client and server data layers.",
            "data": "Writing ETL cleaning scripts in Python or SQL to standardize fields in database tables.",
            "devops": "Adding database migration scripts to clean existing database fields.",
            "ml": "Writing preprocessing scripts to normalize features and clean missing training data.",
            "business": "Standardizing data-entry forms to prevent manual spelling mistakes.",
            "design": "Designing strict input fields and form validation warnings to ensure formatting.",
            "healthcare": "Validating health metrics against medical standards before saving charts.",
            "education": "Setting strict grading criteria formats for course assignments and records."
        }
    },
    {
        "text": "If a cloud service provider goes down, what is your solution?",
        "options": {
            "frontend": "Configuring the app to use cached local storage data and show offline modes.",
            "backend": "Routing API calls to a backup server running in a different datacenter.",
            "fullstack": "Structuring applications to use distributed server setups and client-side failover.",
            "data": "Activating read-only database replicas hosted with secondary providers.",
            "devops": "Deploying container setups to an alternative cloud provider using Terraform scripts.",
            "ml": "Failing back to simpler heuristic models that run on local server hardware.",
            "business": "Evaluating service agreements and calculating financial impacts of the outage.",
            "design": "Designing informative offline states and messaging to reassure users.",
            "healthcare": "Activating local clinical emergency systems so doctors can treat patients offline.",
            "education": "Rescheduling online lessons and migrating materials to backup document stores."
        }
    },
    {
        "text": "When users complain that a form is too difficult to fill, what do you change?",
        "options": {
            "frontend": "Splitting forms into multi-step wizards, adding inline validation, and autocomplete.",
            "backend": "Simplifying database columns to require fewer mandatory fields in the API.",
            "fullstack": "Creating intuitive auto-save states and simplifying data models.",
            "data": "Analyzing form completion funnels to find which specific fields cause drop-offs.",
            "devops": "Configuring automatic address verification APIs to speed up form processes.",
            "ml": "Integrating predictive inputs to pre-fill fields using past user profile details.",
            "business": "Removing non-essential questions to increase registration conversion rates.",
            "design": "Redesigning layout hierarchy, grouping input labels, and improving hints.",
            "healthcare": "Ensuring medical history intake forms are easy for senior patients to navigate.",
            "education": "Simplifying enrollment forms to help students register for classes quickly."
        }
    },
    {
        "text": "How do you resolve conflicts between design assets and code capabilities?",
        "options": {
            "frontend": "Working with designers to adapt styles using standard CSS flex/grid layout constraints.",
            "backend": "Adjusting API response schemas to support complex visual component states.",
            "fullstack": "Refactoring system features to balance aesthetic needs with server scalability.",
            "data": "Translating visual reporting specs into achievable SQL/database calculations.",
            "devops": "Ensuring frontend asset build configurations can support heavy graphic designs.",
            "ml": "Adapting UI features to match dynamic machine learning outputs.",
            "business": "Deciding whether premium styling justifies the extra software development hours.",
            "design": "Modifying visual layouts to use existing UI component frameworks.",
            "healthcare": "Prioritizing medical utility and screen safety over premium visuals.",
            "education": "Ensuring educational materials remain accessible for all student devices."
        }
    },
    # 3. Development Tools and Environment (21-30)
    {
        "text": "Which framework or library would you prefer to explore in depth?",
        "options": {
            "frontend": "Next.js, SolidJS, or Svelte for building efficient user interfaces.",
            "backend": "FastAPI, ASP.NET Core, or Spring Boot for writing high-performance APIs.",
            "fullstack": "Remix or Next.js for managing unified client-server architecture styles.",
            "data": "dbt, Apache Airflow, or Kafka for orchestrating streaming data warehouses.",
            "devops": "Ansible, Terraform, or Kubernetes for automating server systems.",
            "ml": "Hugging Face, PyTorch Lightning, or LangChain for training intelligent agents.",
            "business": "Amplitude, Salesforce, or Productboard for analyzing business growth.",
            "design": "Figma design system kits, Webflow, or Adobe Creative Cloud features.",
            "healthcare": "Epic EHR APIs, FHIR standards, or medical imaging analysis toolkits.",
            "education": "Articulate Storyline, Moodle plugins, or digital lesson design tools."
        }
    },
    {
        "text": "How do you prefer to manage version control branch merges?",
        "options": {
            "frontend": "Reviewing UI component changes and checking visual layout output files.",
            "backend": "Verifying database migrations, API changes, and server controller logic.",
            "fullstack": "Reviewing PRs for both client and database code to prevent integration bugs.",
            "data": "Reviewing SQL schema update files, dataset scripts, and query logic changes.",
            "devops": "Writing strict CI/CD pull request tests to auto-validate pipeline files.",
            "ml": "Tracking model code changes along with training datasets and output versions.",
            "business": "Reviewing release notes to check feature readiness for business schedules.",
            "design": "Comparing Figma file version history to ensure visual layout consistency.",
            "healthcare": "Applying dual-signature code approvals to verify health compliance rules.",
            "education": "Updating documentation files and class guides to reflect system changes."
        }
    },
    {
        "text": "What type of development environment configuration is most important to you?",
        "options": {
            "frontend": "Fast hot-reloading preview tools, CSS styling linters, and browser dev extensions.",
            "backend": "Local database containers, API debug tooling, and server log monitors.",
            "fullstack": "Unified workspace configurations running both frontend and backend servers together.",
            "data": "Jupyter environments, SQL editor consoles, and data connection configurations.",
            "devops": "Terminal interfaces, shell script tools, SSH utilities, and docker setups.",
            "ml": "GPU computing servers, model weight repositories, and tensor plotting libraries.",
            "business": "Project planning boards, product analytics setups, and spreadsheet workspaces.",
            "design": "Clean canvas layouts, vector editing settings, typography kits, and graphics cards.",
            "healthcare": "Secure encrypted test sandbox environments containing mock patient records.",
            "education": "Student view emulation tools, quiz test suites, and curriculum planning grids."
        }
    },
    {
        "text": "If you could automate one manual task in your daily schedule, what would it be?",
        "options": {
            "frontend": "Converting designer Figma frames into clean React component code structures.",
            "backend": "Generating API route definitions and database CRUD operations automatically.",
            "fullstack": "Scaffolding backend tables and frontend inputs from a single schema spec.",
            "data": "Running routine data cleaning scripts and exporting reports to dashboards.",
            "devops": "Deploying codebase updates to production servers and verifying server health.",
            "ml": "Labeling training data inputs and logging training performance charts.",
            "business": "Creating slides from sales metrics and emailing progress summaries to managers.",
            "design": "Exporting graphic assets in multiple formats and sizes for developer use.",
            "healthcare": "Validating clinical document codes against international health regulations.",
            "education": "Grading multiple-choice student quizzes and updating class grade spreadsheets."
        }
    },
    {
        "text": "What is your preference regarding software containerization (e.g., Docker)?",
        "options": {
            "frontend": "Using container builds to guarantee standard browser rendering outputs across teams.",
            "backend": "Packaging API engines and databases to run consistently on any computer.",
            "fullstack": "Structuring full stack web app containers to simplify workspace setup.",
            "data": "Containerizing database staging steps and analysis services for portability.",
            "devops": "Writing custom Dockerfiles, minimizing image weights, and orchestrating containers.",
            "ml": "Containerizing machine learning inference services to run with GPU drivers.",
            "business": "Ensuring team software license costs remain low and workflows standard.",
            "design": "Using web-based prototyping tools instead of setting up local development tools.",
            "healthcare": "Securing health containers to isolate patient data pipelines from external networks.",
            "education": "Using simple pre-configured containers to let students run coding tasks instantly."
        }
    },
    {
        "text": "When evaluating a third-party code package, what is your top criteria?",
        "options": {
            "frontend": "Bundle weight size, support for responsive styles, and accessibility tags.",
            "backend": "Execution speed, database transaction safety, and API concurrency support.",
            "fullstack": "Whether the library works smoothly on both client-side and server-side runtimes.",
            "data": "Data processing speed limits, file formats supported, and aggregation memory use.",
            "devops": "License security, maintenance activity, and how easily it deploys in containers.",
            "ml": "Model accuracy benchmarks, model weight weights, and pipeline integration support.",
            "business": "Implementation cost, commercial licenses, and timeline delivery improvements.",
            "design": "Visual aesthetic flexibility, layout templates, and design kit support.",
            "healthcare": "Medical data security certs, audit trail logging, and regulatory safety approval.",
            "education": "Simplicity of use, documentation quality, and learning curves for students."
        }
    },
    {
        "text": "How do you prefer to test your application code?",
        "options": {
            "frontend": "Writing frontend unit tests for button clicks, form fills, and page navigations.",
            "backend": "Testing API request formats, database mock outputs, and controller logic code.",
            "fullstack": "Writing end-to-end integration tests that cover everything from UI to database.",
            "data": "Validating data types, checking for null values, and testing query speed performance.",
            "devops": "Setting up automatic pipeline tests that check server code before building.",
            "ml": "Using cross-validation splits to verify ML model prediction accuracy.",
            "business": "Testing software usability with user focus groups and reviewing business goals.",
            "design": "Conducting interactive user reviews, A/B layouts, and accessibility checks.",
            "healthcare": "Conducting medical validation checks to protect patient clinical diagnostics.",
            "education": "Creating student pilot tests to ensure curriculum lessons are clear and easy."
        }
    },
    {
        "text": "What type of technical blog or documentation article appeals to you most?",
        "options": {
            "frontend": "Articles about modern CSS features, responsive layout techniques, and JS libraries.",
            "backend": "Deep dives into distributed database systems, API structures, and caching logic.",
            "fullstack": "Tutorials on building and launching dynamic full-stack SaaS apps from scratch.",
            "data": "Guides on building large-scale data warehouses and optimizing SQL scripts.",
            "devops": "Posts detailing cloud infrastructure automation, CI/CD pipelines, and server security.",
            "ml": "Research papers explaining new neural network layers and AI model performance.",
            "business": "Studies explaining product-led growth strategies, user metrics, and business ROI.",
            "design": "Aesthetic design system trends, usability metrics, and prototype layout styles.",
            "healthcare": "Regulatory updates for health software and clinical data exchange standards.",
            "education": "Pedagogical guidelines, educational technology trends, and training frameworks."
        }
    },
    {
        "text": "How do you feel about command-line interfaces (CLI) versus graphic user interfaces (GUI)?",
        "options": {
            "frontend": "Prefer GUIs to preview pages, but use CLI for running component build commands.",
            "backend": "Prefer CLI for script automation, but use GUI database tools to browse tables.",
            "fullstack": "Comfortable switching between CLI server logs and GUI web inspector panels.",
            "data": "Using terminal commands for database ETL, but GUI dashboards to check insights.",
            "devops": "Strongly prefer command-line terminals and configuration scripts for server work.",
            "ml": "Running model training scripts from command shells, but using GUI charts to trace loss.",
            "business": "Prefer visual business tools, boards, and analytics chart builders.",
            "design": "Mainly use visual canvas environments, layout editors, and prototype spaces.",
            "healthcare": "Rely on highly secure graphical software designed for medical professionals.",
            "education": "Using visual, interactive tools to make learning friendly and clear for students."
        }
    },
    {
        "text": "What is your strategy for maintaining code quality in a codebase?",
        "options": {
            "frontend": "Enforcing styling linters, component file structures, and responsive layouts.",
            "backend": "Enforcing database schemas, API documentation, and controller test coverage.",
            "fullstack": "Maintaining consistent types and models across client and server layers.",
            "data": "Creating clear data schema schemas, dictionary files, and testing query outputs.",
            "devops": "Using infrastructure templates and automatic container vulnerability scans.",
            "ml": "Tracking model training configurations, data splits, and model scoring histories.",
            "business": "Documenting product goals, user feedback flows, and metric calculations.",
            "design": "Building design components, color guidelines, and page template guides.",
            "healthcare": "Implementing system audit trails and safety check protocols in coding.",
            "education": "Keeping student guides clear, documenting lesson structures, and grading rules."
        }
    },
    # 4. System Architecture and Logic (31-40)
    {
        "text": "When architecting a system, how do you handle state management?",
        "options": {
            "frontend": "Using client-side state managers like Redux or Context API to synchronize UI views.",
            "backend": "Designing database records, session storage keys, and caching structures.",
            "fullstack": "Synchronizing client UI variables with database tables across API networks.",
            "data": "Maintaining data pipelines, temporal tables, and data version controls.",
            "devops": "Storing configuration settings in vault servers and environment configurations.",
            "ml": "Saving training parameters, feature scales, and neural layer weights.",
            "business": "Tracking business KPIs, budget targets, and market performance records.",
            "design": "Designing clear navigation flows so users always know their current step in the app.",
            "healthcare": "Securing patient records so clinical state updates write safely to databases.",
            "education": "Tracking student lesson progression, quiz completions, and skill histories."
        }
    },
    {
        "text": "If you had to design a messaging system, what would be the core focus?",
        "options": {
            "frontend": "Building real-time message bubbles, typing indicators, and instant scroll updates.",
            "backend": "Managing message delivery databases, user sockets, and REST API queues.",
            "fullstack": "Designing both front-end chat views and back-end socket server integrations.",
            "data": "Analyzing messaging traffic, user interaction counts, and saving chat history files.",
            "devops": "Setting up message broker servers like RabbitMQ or Kafka for reliable routing.",
            "ml": "Developing text analysis models to auto-moderate comments or suggest quick responses.",
            "business": "Assessing chat features to increase user retention and customer help response.",
            "design": "Designing clear chat icons, text fonts, and bubble color layouts for users.",
            "healthcare": "Applying secure clinical encryption to protect private doctor-patient messages.",
            "education": "Building collaborative student chat sections with clear group learning layouts."
        }
    },
    {
        "text": "How do you ensure a system can handle multiple languages (internationalization)?",
        "options": {
            "frontend": "Using translation lookup hooks to swap text strings on pages dynamically.",
            "backend": "Designing localized databases that fetch translations based on header languages.",
            "fullstack": "Configuring both UI translation labels and server-side locale database fields.",
            "data": "Managing global datasets and converting timezone tables to UTC standard formats.",
            "devops": "Configuring global content delivery servers (CDN) to route files based on locale.",
            "ml": "Tuning multi-language translation models and testing language tokenizers.",
            "business": "Analyzing international customer markets and pricing strategy requirements.",
            "design": "Designing layouts that adapt gracefully to varying text lengths in foreign scripts.",
            "healthcare": "Providing clear translations for patient symptoms and clinical instructions.",
            "education": "Structuring education paths to easily localize lectures and learning guides."
        }
    },
    {
        "text": "What is your strategy when designing database relationships?",
        "options": {
            "frontend": "Mapping JSON structures directly to UI state components and lists.",
            "backend": "Creating normalized tables with primary/foreign keys and index optimizations.",
            "fullstack": "Ensuring database tables match with server controllers and frontend models.",
            "data": "Designing dimensional models, star schemas, and aggregate analytics tables.",
            "devops": "Setting up database clusters, automated replication configs, and storage files.",
            "ml": "Extracting database rows into flat feature arrays for training models.",
            "business": "Ensuring database design matches product metrics and reporting needs.",
            "design": "Mapping user categories to interface layouts and clear grouping patterns.",
            "healthcare": "Structuring health databases to connect clinical diagnoses with medication logs.",
            "education": "Mapping student course enrollment keys to teacher assignment records."
        }
    },
    {
        "text": "How do you design a system to support multiple user roles (e.g., admin, student, guest)?",
        "options": {
            "frontend": "Hiding or showing navigation tabs and buttons based on user permissions.",
            "backend": "Creating role-based access middleware and validating JWT scopes on routes.",
            "fullstack": "Configuring client-side route guards and back-end authorization middleware.",
            "data": "Setting database row-level security policies to restrict data access by role.",
            "devops": "Managing IAM access permissions, API keys, and security group privileges.",
            "ml": "Designing personal models customized to different types of user behavior.",
            "business": "Defining target user tiers, workspace rights, and subscription pricing packages.",
            "design": "Creating different page templates custom-suited for administrators and end-users.",
            "healthcare": "Restricting medical chart edits to doctors while leaving them read-only for nurses.",
            "education": "Providing teachers with gradebook inputs and students with lesson views."
        }
    },
    {
        "text": "What is your approach to handling system notification systems?",
        "options": {
            "frontend": "Building sliding toast alerts, notification bell counts, and animation cards.",
            "backend": "Triggering email APIs, managing database event logs, and web-push routes.",
            "fullstack": "Connecting server notification tables to interactive client-side alert tabs.",
            "data": "Tracking notification open rates, delivery counts, and click-through analytics.",
            "devops": "Setting up server status alerts and SMS notification gateways for server health.",
            "ml": "Building smart filters to predict and send only high-priority alerts to users.",
            "business": "Designing notification workflows to encourage customer return rates.",
            "design": "Designing clean warning cards, alert colors, and layout hierarchies for notifications.",
            "healthcare": "Setting up real-time hospital alerts for patient heart rates or medical updates.",
            "education": "Sending homework deadline reminders and teacher feedback alerts to student feeds."
        }
    },
    {
        "text": "How do you structure microservices communication?",
        "options": {
            "frontend": "Fetching data inputs from multiple microservice APIs using frontend gateway routes.",
            "backend": "Setting up HTTP clients, REST endpoints, and gRPC routes between servers.",
            "fullstack": "Designing full features that interact across different backend service APIs.",
            "data": "Consolidating data logs from multiple services into a single database.",
            "devops": "Configuring service meshes, container networking, and message brokers.",
            "ml": "Serving models as distinct microservices that handle prediction API requests.",
            "business": "Evaluating organizational budgets and developer team alignments per service.",
            "design": "Designing matching interface states when services fail to load.",
            "healthcare": "Ensuring patient health exchange between services complies with safety laws.",
            "education": "Isolating educational content microservices from grading history databases."
        }
    },
    {
        "text": "What is your main target when planning database indexing?",
        "options": {
            "frontend": "Reducing loading spinners and ensuring lists update instantly.",
            "backend": "Accelerating server lookup queries and minimizing API response latency.",
            "fullstack": "Improving full stack feature speed by index optimization on tables.",
            "data": "Optimizing complex query scripts that analyze millions of rows.",
            "devops": "Minimizing database server CPU load and disk read operations.",
            "ml": "Speeding up training data load runs from hard storage arrays.",
            "business": "Improving speed performance to increase customer happiness scores.",
            "design": "Ensuring searching for layout assets returns results instantly.",
            "healthcare": "Ensuring medical history records load instantly during doctor visits.",
            "education": "Optimizing database search so students can quickly find lessons."
        }
    },
    {
        "text": "How do you design for offline capability in applications?",
        "options": {
            "frontend": "Configuring Service Workers, PWA configurations, and indexedDB local storage.",
            "backend": "Designing data synchronization APIs and handling merge conflict records.",
            "fullstack": "Implementing local state synchronization logic with backend databases.",
            "data": "Handling offline data collection uploads and standardizing timestamp logs.",
            "devops": "Setting up caching proxies and configuring offline staging test systems.",
            "ml": "Developing lightweight client-side models that process inputs without network calls.",
            "business": "Validating app utility in areas with low internet connectivity.",
            "design": "Designing clear visual indicators showing offline status and sync status.",
            "healthcare": "Ensuring doctors can log clinical details even when clinic Wi-Fi fails.",
            "education": "Building offline learning modes that download lessons to student devices."
        }
    },
    {
        "text": "When designing API payloads, what do you value most?",
        "options": {
            "frontend": "Clean, flat JSON objects that easily bind to visual UI components.",
            "backend": "Secure types, minimal data weight, and consistent response formats.",
            "fullstack": "Payloads that balance ease of UI integration with server query speed.",
            "data": "Including detailed audit parameters and standardized database keys.",
            "devops": "Enforcing API contract validations and monitoring payload sizes.",
            "ml": "Formatting arrays into structured inputs suitable for neural processors.",
            "business": "Including parameters needed to track product growth metrics.",
            "design": "Ensuring output values map cleanly to visual page labels and widgets.",
            "healthcare": "Enforcing standardized medical record formats (like HL7/FHIR packages).",
            "education": "Including clear quiz scoring metadata and assignment feedback text."
        }
    },
    # 5. Data Handling and Processing (41-50)
    {
        "text": "What is your primary goal when writing a script to process a dataset?",
        "options": {
            "frontend": "Transforming data attributes to match UI dropdown formats and visual charts.",
            "backend": "Validating data integrity constraints before writing updates to databases.",
            "fullstack": "Passing form data between client elements and database tables securely.",
            "data": "Cleaning messy data columns, aggregating rows, and exporting clean metrics.",
            "devops": "Automating dataset backup scripts and configuring storage storage locations.",
            "ml": "Normalizing dataset features and preparing training-test arrays.",
            "business": "Extracting insights to make business decisions and evaluate revenue.",
            "design": "Ensuring data fields map logically to user interface layouts.",
            "healthcare": "Extracting patient clinical symptoms to aid in medical diagnosis.",
            "education": "Aggregating student quiz scores to identify curriculum learning gaps."
        }
    },
    {
        "text": "How do you prefer to store system log files?",
        "options": {
            "frontend": "Logging client-side browser errors to monitoring APIs (like Sentry).",
            "backend": "Writing structured JSON files with database logs and API error traces.",
            "fullstack": "Consolidating client-side exceptions and server errors into one database.",
            "data": "Storing pipeline logs in database tables to analyze system speed.",
            "devops": "Streaming server logs to central dashboards like Elasticsearch or CloudWatch.",
            "ml": "Logging model training loss values and validation accuracies over time.",
            "business": "Tracking customer activity logs to analyze feature usage trends.",
            "design": "Logging user path choices to map the most popular page navigation layouts.",
            "healthcare": "Maintaining strict HIPAA-compliant audit logs of patient record access.",
            "education": "Tracking lesson study hours and homework submission timelines."
        }
    },
    {
        "text": "When working with unstructured data (like free text), what do you do?",
        "options": {
            "frontend": "Formatting raw text with HTML tags and highlighting matching search words.",
            "backend": "Storing text strings in database document fields and writing simple search queries.",
            "fullstack": "Creating UI text editors and saving rich-text configurations to servers.",
            "data": "Parsing text strings with regular expressions to extract structured fields.",
            "devops": "Setting up indexing systems (like Elasticsearch) to index text databases.",
            "ml": "Tokenizing text, training embeddings, and applying NLP models.",
            "business": "Reading feedback surveys to identify customer sentiment and feature needs.",
            "design": "Analyzing user interview text notes to discover user pain points.",
            "healthcare": "Analyzing patient clinic notes to capture symptom keywords and diagnoses.",
            "education": "Reading student essays to evaluate learning comprehension and progress."
        }
    },
    {
        "text": "How do you approach database backups?",
        "options": {
            "frontend": "Storing temporary user settings in browser local storage as a fallback.",
            "backend": "Writing database export scripts and scheduling routine SQL dumps.",
            "fullstack": "Configuring auto-save triggers in forms to prevent user data loss.",
            "data": "Structuring data warehouse staging and historical snapshot tables.",
            "devops": "Scheduling automated, encrypted cloud backups with failover systems.",
            "ml": "Saving training checkpoints and dataset versions for training runs.",
            "business": "Estimating business risk costs of data loss to plan backup storage budgets.",
            "design": "Exporting graphic layouts and design system components to cloud files.",
            "healthcare": "Securing redundant, encrypted copies of medical records off-site.",
            "education": "Ensuring student academic files are archived safely at the school term end."
        }
    },
    {
        "text": "What type of database system do you prefer?",
        "options": {
            "frontend": "Client storage databases like IndexedDB, LocalStorage, or key-value stores.",
            "backend": "Relational databases like PostgreSQL or MySQL for structured data.",
            "fullstack": "Flexible databases like MongoDB or PostgreSQL that support rapid app coding.",
            "data": "Analytical column databases like Snowflake, BigQuery, or Redshift.",
            "devops": "Distributed key-value systems like Redis, DynamoDB, or cluster databases.",
            "ml": "Vector databases like Pinecone, Milvus, or FAISS for embeddings search.",
            "business": "Integrated customer data platforms like Salesforce or Hubspot.",
            "design": "Visual database libraries and shared design style repositories.",
            "healthcare": "Secure medical database systems designed for patient record compliance.",
            "education": "Student information systems and course progress tracking databases."
        }
    },
    {
        "text": "How do you handle real-time streaming data?",
        "options": {
            "frontend": "Establishing WebSocket connections to update UI components instantly.",
            "backend": "Building socket API servers and event listener endpoints.",
            "fullstack": "Integrating real-time event systems with both frontend views and backend tables.",
            "data": "Writing stream analysis scripts with Apache Spark or Flink.",
            "devops": "Configuring messaging brokers like Kafka or AWS Kinesis to route streams.",
            "ml": "Feeding real-time data inputs to models for instant prediction scoring.",
            "business": "Monitoring real-time business sales metrics to adjust marketing spend.",
            "design": "Designing real-time status UI indicators and system alert banners.",
            "healthcare": "Monitoring live patient vitals and generating immediate clinical warnings.",
            "education": "Tracking live student quiz participation and online classroom metrics."
        }
    },
    {
        "text": "When a dataset has missing values, what is your standard solution?",
        "options": {
            "frontend": "Rendering elegant fallback text labels (like 'N/A') in missing UI card fields.",
            "backend": "Using database defaults and ensuring API schemas accept nullable parameters.",
            "fullstack": "Validating form fields to prevent missing inputs from saving to databases.",
            "data": "Running imputation scripts or ignoring empty rows depending on data policies.",
            "devops": "Checking if database migration scripts failed to fill legacy columns.",
            "ml": "Applying feature engineering to replace missing data with mean, median, or predicted values.",
            "business": "Surveying clients to complete customer files and improve business data.",
            "design": "Designing layouts that adapt gracefully when user profile information is incomplete.",
            "healthcare": "Marking missing diagnostic charts as urgent to request immediate nurse checks.",
            "education": "Setting up makeup quiz options for students who missed exam dates."
        }
    },
    {
        "text": "What do you look for when analyzing user engagement metrics?",
        "options": {
            "frontend": "Analyzing click heatmaps, scroll depth, and page performance times.",
            "backend": "Evaluating database transaction logs and API endpoint call rates.",
            "fullstack": "Analyzing user flow patterns across the frontend page routes to backend database updates.",
            "data": "Aggregating daily active user counts, cohort retention, and funnel drop-off metrics.",
            "devops": "Analyzing server bandwidth spikes, concurrent requests, and session counts.",
            "ml": "Feeding interaction features to recommendation systems to predict retention.",
            "business": "Evaluating customer acquisition costs, business revenue, and user lifetime value.",
            "design": "Checking visual clarity, usability tests, and identifying navigation pain points.",
            "healthcare": "Evaluating system usage rates by clinical staff to improve hospital software.",
            "education": "Analyzing student lesson completion metrics and homework grade levels."
        }
    },
    {
        "text": "How do you handle data privacy regulations (like GDPR)?",
        "options": {
            "frontend": "Creating cookie consent bars, privacy check settings, and option toggles.",
            "backend": "Implementing user account deletion routes and database field scrubbing logic.",
            "fullstack": "Building frontend privacy request forms and connecting them to backend database removal scripts.",
            "data": "Writing scripts to anonymize user identifiers in analytical datasets.",
            "devops": "Ensuring database storage arrays are encrypted at rest and locked down.",
            "ml": "Removing personally identifiable features from model training inputs.",
            "business": "Writing legal compliance policies and scheduling data safety audits.",
            "design": "Designing clear privacy notices and helping users manage personal settings easily.",
            "healthcare": "Applying HIPAA standards to restrict medical data access to authorized clinicians.",
            "education": "Securing student academic files to comply with educational privacy rules."
        }
    },
    {
        "text": "What is your strategy for compiling reports?",
        "options": {
            "frontend": "Building interactive visual chart libraries (like Recharts) with modern designs.",
            "backend": "Writing efficient backend queries to compile database fields into JSON summaries.",
            "fullstack": "Developing PDF download buttons and connecting them to backend document engines.",
            "data": "Orchestrating BI software dashboards and automated SQL reporting pipelines.",
            "devops": "Automating weekly server cost charts and performance report emails.",
            "ml": "Generating model validation matrices, ROC curves, and bias reports.",
            "business": "Writing executive summaries, financial charts, and business strategy slides.",
            "design": "Creating infographic designs, research summaries, and visual brand kits.",
            "healthcare": "Formatting clinical reports for medical diagnosis and health regulatory audits.",
            "education": "Creating student report cards and lesson feedback forms for parent reviews."
        }
    },
    # 6. Design and Aesthetics (51-60)
    {
        "text": "When choosing colors for a website, what do you consider first?",
        "options": {
            "frontend": "Enforcing HSL variables, CSS theme states, and color accessibility requirements.",
            "backend": "Saving user color mode choices (light/dark) in user database records.",
            "fullstack": "Implementing color theme toggles in both frontend components and server configs.",
            "data": "Using distinct colors in data charts to make metrics readable for viewers.",
            "devops": "Using color alerts in terminal monitoring dashboards to show server errors.",
            "ml": "Mapping neural output features to color spectrums in visualization plots.",
            "business": "Using company colors to build brand trust and improve customer clicks.",
            "design": "Creating harmonious color palettes, testing contrast ratios, and building brand styles.",
            "healthcare": "Using medical-standard UI colors to ensure clinical alerts are visible.",
            "education": "Using warm, inviting color schemes to make learning platforms friendly."
        }
    },
    {
        "text": "How do you define layout spacing in your projects?",
        "options": {
            "frontend": "Using unified CSS variables, padding helpers, and tailwind spacing grids.",
            "backend": "We do not handle visual layout spacing, but structure database models.",
            "fullstack": "Developing reusable UI component frameworks that enforce standard layouts.",
            "data": "Structuring dashboard layouts to display analytical metrics logically.",
            "devops": "Configuring network configurations with clear IP range spacing partitions.",
            "ml": "Calculating distance spacing in vector spaces for classification models.",
            "business": "Scoping layout specifications for presentation slides and marketing flyers.",
            "design": "Creating layout grids, defining padding rules, and prototyping spacing systems.",
            "healthcare": "Structuring clinic workspace screens to display critical patient stats clearly.",
            "education": "Designing clear spacing in worksheets to make learning easy for students."
        }
    },
    {
        "text": "When designing a user interface, what type of navigation style do you use?",
        "options": {
            "frontend": "Implementing responsive drawer panels, dropdown menus, and smooth page transitions.",
            "backend": "Validating routing parameters and path redirections in backend controllers.",
            "fullstack": "Building multi-level menus and connecting routes to user permission databases.",
            "data": "Structuring data tables to allow easy pagination and metric drilling.",
            "devops": "Setting up domain routing, subdomains, and API proxy pathways.",
            "ml": "Creating graph paths for recommendation models to suggest articles.",
            "business": "Optimizing user flows to guide customers to purchase screens.",
            "design": "Drawing wireframes, user journeys, and choosing navigation systems.",
            "healthcare": "Designing fast menu pathways to let doctors access clinical charts immediately.",
            "education": "Creating sequential step progress paths to guide students through classes."
        }
    },
    {
        "text": "What is your approach to handling typography (fonts)?",
        "options": {
            "frontend": "Loading custom Google Fonts, setting fallback font tags, and optimizing load speed.",
            "backend": "We focus on data schemas rather than font rendering details.",
            "fullstack": "Configuring styling packages that deliver fonts cleanly across the app.",
            "data": "Choosing clean, highly legible font sizes for dashboard charts.",
            "devops": "Configuring terminal text fonts to ensure clear reading of server scripts.",
            "ml": "Preprocessing character sets and text vectors for NLP models.",
            "business": "Choosing brand fonts that communicate corporate style and professionalism.",
            "design": "Defining font pairings, styling hierarchies, font-weights, and line heights.",
            "healthcare": "Enforcing highly legible fonts to prevent clinical dosage reading mistakes.",
            "education": "Selecting kid-friendly or highly readable fonts to assist dyslexic learners."
        }
    },
    {
        "text": "How do you approach the styling of custom icons?",
        "options": {
            "frontend": "Importing SVG icons, writing hover animations, and dynamic colors.",
            "backend": "We write API logic rather than graphic styling code.",
            "fullstack": "Adding icon libraries to core bundles and wrapping them in UI components.",
            "data": "Using standard icons to label datasets in dashboard visualizations.",
            "devops": "Configuring server status icons in cloud console monitoring widgets.",
            "ml": "Analyzing visual feature arrays in computer vision model training.",
            "business": "Reviewing icon designs to verify alignment with brand identity guides.",
            "design": "Designing custom vector icons, aligning them to grids, and matching line weights.",
            "healthcare": "Using universal medical icons (like red crosses) to label emergency charts.",
            "education": "Creating fun, colorful icons to reward student learning milestones."
        }
    },
    {
        "text": "When designing a profile page, what elements do you prioritize?",
        "options": {
            "frontend": "Creating responsive user avatars, clean layouts, and tabbed menus.",
            "backend": "Designing user profile tables, API update routes, and field validation.",
            "fullstack": "Connecting profile form components to database update routes securely.",
            "data": "Structuring user tables to store activity history metrics.",
            "devops": "Securing profile update endpoints and storage buckets containing avatar images.",
            "ml": "Building user models that predict interests based on profile details.",
            "business": "Analyzing profile completeness metrics to improve customer engagement.",
            "design": "Designing a premium visual layout that communicates user identity clearly.",
            "healthcare": "Ensuring patient profile charts display allergy warnings prominently.",
            "education": "Displaying student course grades, skill badges, and progress reports."
        }
    },
    {
        "text": "How do you design a dashboard interface?",
        "options": {
            "frontend": "Implementing grid widgets, responsive cards, and dynamic theme colors.",
            "backend": "Writing aggregate database queries to fetch dashboard statistics quickly.",
            "fullstack": "Connecting front-end chart components to real-time database update endpoints.",
            "data": "Selecting critical charts, KPIs, and designing data visualization layouts.",
            "devops": "Configuring server performance monitoring dashboards (like Grafana).",
            "ml": "Plotting model training metrics, predictions, and feature scores.",
            "business": "Analyzing dashboard metrics to track business growth and monthly sales.",
            "design": "Wireframing the layout hierarchy, choosing color schemes, and user testing.",
            "healthcare": "Structuring clinical dashboards to display live patient vitals clearly.",
            "education": "Designing school portals showing student grades and class schedules."
        }
    },
    {
        "text": "When a page layout looks cluttered, what is your standard solution?",
        "options": {
            "frontend": "Applying CSS spacing rules, using dropdown accordions, and hiding secondary items.",
            "backend": "Reducing API payload weights and sending only requested database rows.",
            "fullstack": "Refactoring features to prioritize core details and simplify screen states.",
            "data": "Filtering analytics charts to display only key performance indicators.",
            "devops": "Simplifying cloud infrastructure diagrams to focus on main service connections.",
            "ml": "Applying dimensionality reduction to simplify complex feature data sets.",
            "business": "Restructuring product features to focus on the core value proposition.",
            "design": "Increasing empty whitespace, simplifying typographic scales, and grouping elements.",
            "healthcare": "Redesigning clinical charts to highlight only critical patient symptoms.",
            "education": "Simplifying lesson layouts to prevent cognitive overload for students."
        }
    },
    {
        "text": "What is your approach to designing dark modes?",
        "options": {
            "frontend": "Using CSS variables, matching dark themes, and writing smooth theme transitions.",
            "backend": "Saving dark mode preferences in user database tables.",
            "fullstack": "Configuring theme providers that synchronize dark mode states across layers.",
            "data": "Adjusting data visualization chart colors to stand out on dark backgrounds.",
            "devops": "Configuring terminal logs and cloud dashboards to run in dark mode.",
            "ml": "Visualizing neural model matrices in dark-theme research presentation slides.",
            "business": "Checking if dark mode options increase user session durations.",
            "design": "Selecting low-contrast dark background colors and adjusting text legibility.",
            "healthcare": "Creating dark mode clinical screens to reduce eye strain for night-shift doctors.",
            "education": "Designing dark mode features to make nighttime study comfortable for students."
        }
    },
    {
        "text": "How do you test user interface usability?",
        "options": {
            "frontend": "Running local browser automation tests to simulate user clicks and form fills.",
            "backend": "Monitoring API performance and error rates during user sessions.",
            "fullstack": "Tracking user journey tests from client actions to backend database changes.",
            "data": "Analyzing funnel charts to find where users drop off in registration processes.",
            "devops": "Monitoring backend system loads during high user activity sessions.",
            "ml": "A/B testing machine learning outputs to see which predictions users click more.",
            "business": "Hosting focus groups, analyzing satisfaction surveys, and tracking conversion metrics.",
            "design": "Conducting user tests, recording screens, and gathering feedback on layouts.",
            "healthcare": "Testing clinical software safety with medical staff to avoid dosage errors.",
            "education": "Testing learning portals with students to verify that lesson paths are clear."
        }
    },
    # 7. Team Collaboration and Roles (61-70)
    {
        "text": "In a team meeting, which project update concerns you most?",
        "options": {
            "frontend": "Changes to visual designs that require rebuilding page layout code.",
            "backend": "Changes to API specifications that require rewriting database endpoints.",
            "fullstack": "Integration mismatches between the client app and the server logic.",
            "data": "Updates to database models that require rebuilding ETL pipelines.",
            "devops": "System deployment errors or cloud provider subscription cost increases.",
            "ml": "Model validation accuracy drops or training dataset corruption.",
            "business": "Project delivery delays that push back target product launch dates.",
            "design": "Brand strategy shifts that require redesigning the visual styling guide.",
            "healthcare": "System bugs that could risk medical data security or clinical safety.",
            "education": "Student feedback indicating that learning guides are confusing."
        }
    },
    {
        "text": "What type of tasks do you prefer to delegate to others?",
        "options": {
            "frontend": "Backend API database design and network configuration work.",
            "backend": "Frontend page styling, CSS animations, and browser rendering optimization.",
            "fullstack": "We handle both, but delegate devops setup or graphic asset design.",
            "data": "Designing graphics or configuring server hosting pipelines.",
            "devops": "Writing application feature code and designing visual layouts.",
            "ml": "Building frontend dashboard views and writing standard business logic.",
            "business": "Technical coding, database queries, and graphic design tasks.",
            "design": "Software development coding and database table configurations.",
            "healthcare": "Non-compliant admin tasks to focus on patient safety systems.",
            "education": "System operations work to focus on lesson curriculum design."
        }
    },
    {
        "text": "How do you explain technical issues to non-technical stakeholders?",
        "options": {
            "frontend": "Showing browser mockups and illustrating user interaction paths.",
            "backend": "Using analogies about server communication and database storage rooms.",
            "fullstack": "Explaining the technical flow from user button clicks to database storage.",
            "data": "Using clear data charts, trend lines, and visual summaries.",
            "devops": "Explaining cloud infrastructure costs and server reliability using uptime scores.",
            "ml": "Describing AI models as smart matching systems trained on examples.",
            "business": "Translating technical issues into customer impact and financial costs.",
            "design": "Presenting visual user journeys and explaining layout usability.",
            "healthcare": "Focusing on patient care quality, data privacy, and clinical safety impact.",
            "education": "Explaining how changes affect student learning progress and course ease."
        }
    },
    {
        "text": "What is your contribution during project brainstorming sessions?",
        "options": {
            "frontend": "Suggesting clean UI interactions, styles, and front-end tools.",
            "backend": "Advising on system scalability, database structures, and API routes.",
            "fullstack": "Proposing end-to-end architectures and planning rapid feature prototyping.",
            "data": "Identifying key metrics we should track and analytical insights we can leverage.",
            "devops": "Planning server deployment paths, cloud scaling, and server costs.",
            "ml": "Proposing AI predictions and model integrations to improve features.",
            "business": "Defining target user segments, market fit, and project budget constraints.",
            "design": "Sketching page ideas, user flows, and planning design patterns.",
            "healthcare": "Ensuring all product ideas match medical privacy and clinic compliance laws.",
            "education": "Focusing on course structures, mentoring strategies, and student help."
        }
    },
    {
        "text": "How do you prefer to handle project code reviews?",
        "options": {
            "frontend": "Checking CSS styling conventions, React component reuse, and responsiveness.",
            "backend": "Reviewing database query efficiency, secure API logic, and C# controller routes.",
            "fullstack": "Verifying that frontend changes line up perfectly with backend endpoints.",
            "data": "Checking SQL syntax efficiency, table indexing, and data script accuracy.",
            "devops": "Reviewing deployment YAML scripts, cloud permissions, and build settings.",
            "ml": "Reviewing preprocessing logic, tensor metrics, and model evaluation files.",
            "business": "Checking if developer changes meet documented product specifications.",
            "design": "Reviewing UI components to verify they match visual Figma specifications.",
            "healthcare": "Double-checking code scripts for patient data safety and encryption.",
            "education": "Ensuring educational instructions and lesson files are simple and clear."
        }
    },
    {
        "text": "What type of team structure matches your preference?",
        "options": {
            "frontend": "Collaborating with designers and client-side engineers on web features.",
            "backend": "Working with database managers and system engineers on secure logic.",
            "fullstack": "Working in agile, cross-functional squads to build end-to-end features.",
            "data": "Working with analytics experts, data engineers, and business planners.",
            "devops": "Working with system administrators and code developers to automate operations.",
            "ml": "Working with research scientists, data experts, and model deployers.",
            "business": "Leading project managers, sales leads, and target clients.",
            "design": "Collaborating with visual designers, copywriters, and user researchers.",
            "healthcare": "Working with medical doctors, clinic directors, and regulatory leads.",
            "education": "Collaborating with teachers, course tutors, and online instruction leads."
        }
    },
    {
        "text": "How do you manage deadlines when a project falls behind?",
        "options": {
            "frontend": "Prioritizing critical UI features and using pre-built style components to save time.",
            "backend": "Simplifying API endpoints and postponing minor database refactoring.",
            "fullstack": "Building a simplified MVP first and scheduling refactors for later phases.",
            "data": "Focusing on main analytics reports first and postponing complex pipelines.",
            "devops": "Automating environment setups to speed up testing and code deployments.",
            "ml": "Using pre-trained models first instead of custom-training neural layers.",
            "business": "Renegotiating project scope, adjust budgets, and communicate changes to clients.",
            "design": "Simplifying layout designs to use standard design kit templates.",
            "healthcare": "Refusing to cut compliance corners, ensuring safety checks remain complete.",
            "education": "Postponing complex course features to ensure core lessons launch on time."
        }
    },
    {
        "text": "What type of feedback motivates you most?",
        "options": {
            "frontend": "Users praising the app interface for looking clean and feeling fast.",
            "backend": "Developers praising the API codebase for being robust and easy to read.",
            "fullstack": "Clients praising the app for being fully featured and working seamlessly.",
            "data": "Management thanking you for data reports that saved corporate budget.",
            "devops": "Teams thanking you for build scripts that made deployment easy.",
            "ml": "Research teams praising the model for achieving high accuracy scores.",
            "business": "Stakeholders celebrating increased user conversion and revenue metrics.",
            "design": "Design communities praising the product styling and usability layout.",
            "healthcare": "Medical professionals saying the system helps them diagnose patients safely.",
            "education": "Students saying that lessons helped them learn a new career skill."
        }
    },
    {
        "text": "When a team member makes a coding mistake, what is your approach?",
        "options": {
            "frontend": "Helping them correct UI styles and explaining responsive design patterns.",
            "backend": "Assisting them to debug database transactions and explaining server logic.",
            "fullstack": "Explaining the integration bugs and showing them how to fix both layers.",
            "data": "Showing them how to fix SQL queries and optimize table joins.",
            "devops": "Helping them fix build scripts and explaining cloud configuration settings.",
            "ml": "Explaining why the model failed validation checks and how to adjust layers.",
            "business": "Assisting them to align their work with documented feature specs.",
            "design": "Showing them how to align UI components with the design system guide.",
            "healthcare": "Ensuring the mistake is logged and corrected to match health privacy laws.",
            "education": "Creating a tutorial guide to explain the technical concepts clearly."
        }
    },
    {
        "text": "How do you document your team's processes?",
        "options": {
            "frontend": "Writing component documentation, UI storybooks, and layout styles.",
            "backend": "Writing Swagger API specs, database dictionaries, and route diagrams.",
            "fullstack": "Writing complete integration guides covering frontend and backend setups.",
            "data": "Creating data dictionaries, ETL charts, and analytics report guides.",
            "devops": "Writing server build files, configuration guides, and backup routines.",
            "ml": "Documenting training configurations, data splits, and model scoring rules.",
            "business": "Writing product briefs, release timelines, and customer feedback metrics.",
            "design": "Publishing design system style kits, font files, and UX persona slides.",
            "healthcare": "Writing safety checklists, encryption rules, and medical audit steps.",
            "education": "Designing training manuals, lecture lesson templates, and grading guides."
        }
    },
    # 8. Professional Goals and Metrics (71-80)
    {
        "text": "Which career milestone appeals to you most?",
        "options": {
            "frontend": "Leading the frontend development of a website used by millions of users.",
            "backend": "Architecting a secure distributed server system processing high traffic volumes.",
            "fullstack": "Launching a successful SaaS product built entirely by yourself.",
            "data": "Discovering a key business trend that pivots company strategy.",
            "devops": "Designing a cloud pipeline that automates all container operations.",
            "ml": "Creating an AI model that sets a new accuracy benchmark.",
            "business": "Reaching a executive role and managing product divisions.",
            "design": "Winning design awards for creating an exceptionally user-friendly product.",
            "healthcare": "Implementing technology that improves healthcare access and patient safety.",
            "education": "Building an educational system that trains thousands of student careers."
        }
    },
    {
        "text": "What type of professional certification would you like to acquire?",
        "options": {
            "frontend": "Advanced UX Engineering, CSS Masters, or React Developer certification.",
            "backend": "Oracle Database Professional, C# Professional, or Java Architect.",
            "fullstack": "Full Stack Software Engineering or Cloud Application Developer certs.",
            "data": "Certified Data Management Professional or Google Data Analyst cert.",
            "devops": "AWS Certified Solutions Architect, Kubernetes Admin, or Terraform Expert.",
            "ml": "TensorFlow Developer Certificate or Machine Learning Engineering cert.",
            "business": "Project Management Professional (PMP) or Certified Product Manager.",
            "design": "Certified UX Designer, Interaction Design, or Figma System Architect.",
            "healthcare": "Certified Health Informatics Professional or HIPAA compliance specialist.",
            "education": "Instructional Design Certificate, Higher Education Teaching, or eLearning specialist."
        }
    },
    {
        "text": "What is the main KPI (Key Performance Indicator) you focus on?",
        "options": {
            "frontend": "First Contentful Paint speed, Cumulative Layout Shift, and user interface scores.",
            "backend": "API response latency, server database uptime, and transaction success rates.",
            "fullstack": "Feature completion timelines, bug counts, and full system performance metrics.",
            "data": "Data report delivery speed, SQL query times, and database metrics accuracy.",
            "devops": "Continuous build success rates, server costs, and time to deploy updates.",
            "ml": "Model training loss, prediction accuracy scores, and model inference speed.",
            "business": "Monthly active users, customer acquisition cost, conversion rate, and ARR.",
            "design": "Task success rates, system usability scores, and user aesthetic feedback.",
            "healthcare": "Clinical record accuracy, clinical safety rates, and health system uptime.",
            "education": "Student graduation rates, course satisfaction ratings, and learning progress."
        }
    },
    {
        "text": "If you could write a technical book, what would the topic be?",
        "options": {
            "frontend": "Designing Responsive Web Applications with Modern JavaScript Frameworks.",
            "backend": "Building Scalable microservices, SQL Databases, and Secure API Routes.",
            "fullstack": "From Zero to Hero: Building and Deploying Complete Full-Stack Web Apps.",
            "data": "Data Warehousing and BI Pipelines for Enterprise Scale Analysis.",
            "devops": "Infrastructure as Code: Automating Cloud Deployments with Kubernetes.",
            "ml": "An Introduction to Training Neural Networks and Computer Vision Models.",
            "business": "Product Strategy: Aligning Engineering Teams with Market Opportunity.",
            "design": "Design Systems: Building Legible, Accessible, and Beautiful Layouts.",
            "healthcare": "Digital Health Informatics: Engineering Secure and Compliant Medical Systems.",
            "education": "E-Learning Pedagogy: Designing Digital Curriculums for Modern Students."
        }
    },
    {
        "text": "What is your approach to professional networking?",
        "options": {
            "frontend": "Sharing UI code snippets, design templates, and frontend tips on Twitter/GitHub.",
            "backend": "Discussing database schemas and API design patterns on tech forums.",
            "fullstack": "Attending local tech startup meetups to discuss full-stack product ideas.",
            "data": "Participating in data science competitions and sharing analytics notebooks.",
            "devops": "Contributing to cloud configuration open-source scripts on GitHub.",
            "ml": "Sharing research reviews, model training scripts, and AI news on LinkedIn.",
            "business": "Attending industry conferences to find partners and discuss business strategies.",
            "design": "Sharing design mockups on Dribbble, Figma libraries, and UX reviews.",
            "healthcare": "Joining health informatics organizations to discuss medical technology updates.",
            "education": "Attending instructional design workshops to share mentoring techniques."
        }
    },
    {
        "text": "Which career growth path interests you most?",
        "options": {
            "frontend": "Becoming a Principal Frontend Architect, specializing in UI design systems.",
            "backend": "Becoming a Systems Architect, managing backend scaling and server security.",
            "fullstack": "Becoming a Technical Co-Founder or Lead Developer of a tech company.",
            "data": "Becoming a Chief Data Officer, directing business analytics and database plans.",
            "devops": "Becoming a Director of Infrastructure, managing cloud environments and networks.",
            "ml": "Becoming a Chief AI Scientist, leading advanced neural network research projects.",
            "business": "Becoming a VP of Product or Chief Executive Officer, defining business plans.",
            "design": "Becoming a Creative Director, managing user design guides and style layouts.",
            "healthcare": "Becoming a Director of Clinical Informatics, managing health system compliance.",
            "education": "Becoming a Learning and Development Director, managing academic training."
        }
    },
    {
        "text": "How do you stay up-to-date with your industry?",
        "options": {
            "frontend": "Reading frontend blogs, checking GitHub repos, and following styling updates.",
            "backend": "Subscribing to database newsletters and reading API optimization guides.",
            "fullstack": "Building personal side-projects using new tech stacks and framework features.",
            "data": "Reading analytics blogs, learning SQL updates, and checking public datasets.",
            "devops": "Monitoring cloud provider release logs and checking server tool updates.",
            "ml": "Reading AI conference papers, Kaggle boards, and model release notes.",
            "business": "Reading business reviews, product case studies, and market analysis papers.",
            "design": "Browsing design sites, studying design system kits, and analyzing layout usability.",
            "healthcare": "Reading medical journals, healthcare technology updates, and privacy laws.",
            "education": "Reading educational research papers, pedagogy guides, and learning technology reviews."
        }
    },
    {
        "text": "What type of legacy do you wish to leave in your work?",
        "options": {
            "frontend": "Creating beautiful, highly accessible web pages that anyone can browse.",
            "backend": "Building reliable, secure backend engines that never failed under high load.",
            "fullstack": "Building systems that solved real problems from start to finish.",
            "data": "Uncovering data insights that helped shape successful organizational directions.",
            "devops": "Creating automated cloud pipelines that made system deployments effortless.",
            "ml": "Developing intelligent models that automated complex human tasks.",
            "business": "Growing a startup into a successful, profitable business division.",
            "design": "Designing iconic, simple products that changed how users interact with technology.",
            "healthcare": "Designing medical software that helped clinicians save patient lives.",
            "education": "Mentoring future professionals and building platforms that spread education."
        }
    },
    {
        "text": "When choosing a new job role, what company benefit is most important?",
        "options": {
            "frontend": "Workspaces with modern visual layout monitors and design system resources.",
            "backend": "Budget for robust local server configurations, databases, and debugging tools.",
            "fullstack": "Opportunities to work across the stack and build entire features independently.",
            "data": "Access to clean datasets, cloud data warehouses, and advanced BI tools.",
            "devops": "Budgets for hosting servers and opportunities to build modern CI/CD systems.",
            "ml": "Access to high-compute GPU instances and neural model training environments.",
            "business": "Profit sharing plans, clear leadership pathways, and business strategy roles.",
            "design": "Design-centric culture, Figma workspaces, and visual prototyping resources.",
            "healthcare": "Support for clinical compliance training and medical safety frameworks.",
            "education": "Opportunities to mentor others, conduct training, and design teaching paths."
        }
    },
    {
        "text": "What is your approach to work-life balance?",
        "options": {
            "frontend": "Polishing styling code during core hours and exploring web styles as a hobby.",
            "backend": "Ensuring server stability so you are rarely called for midnight emergency crashes.",
            "fullstack": "Managing project timelines carefully to avoid late-night integration runs.",
            "data": "Automating data updates to run on schedules without manual supervision.",
            "devops": "Building automated container systems that monitor and auto-heal systems 24/7.",
            "ml": "Leaving model training runs to complete overnight while you sleep.",
            "business": "Delegating technical tasks to focus on strategy planning during business hours.",
            "design": "Gathering layout inspiration from nature and art outside of working hours.",
            "healthcare": "Maintaining strict boundaries to prevent clinical shift burnout.",
            "education": "Scheduling class reviews during school hours to protect training prep time."
        }
    },
    # 9. Product Strategy and Launch (81-90)
    {
        "text": "When launch date is tomorrow, what task is most critical?",
        "options": {
            "frontend": "Double-checking page layout responsiveness, font rendering, and layout shifts.",
            "backend": "Verifying database migration execution and API server routes security.",
            "fullstack": "Running end-to-end integration tests to safety-check core application paths.",
            "data": "Verifying that reporting database tables are collecting system metrics properly.",
            "devops": "Safety-checking production server loads and cloud autoscaling scripts.",
            "ml": "Ensuring prediction models are loaded and serving inferences without latency.",
            "business": "Reviewing marketing plans, client signup links, and billing configurations.",
            "design": "Checking visual assets, icons, layouts, and style consistency.",
            "healthcare": "Verifying medical compliance checks and clinical data encryption keys.",
            "education": "Ensuring lesson materials, test keys, and course guides are uploaded."
        }
    },
    {
        "text": "How do you evaluate if a product launch was successful?",
        "options": {
            "frontend": "Checking if client-side performance times remained fast under high traffic.",
            "backend": "Analyzing API error metrics and database connection health.",
            "fullstack": "Ensuring all product layers functioned correctly and users registered accounts.",
            "data": "Reviewing analytics databases to measure total customer signup metrics.",
            "devops": "Analyzing cloud server costs, CPU metrics, and container stability trends.",
            "ml": "Checking if the model delivered accurate recommendations during user traffic.",
            "business": "Measuring revenue numbers, conversion rates, and return on investment.",
            "design": "Evaluating usability feedback, user reviews, and design layout satisfaction.",
            "healthcare": "Ensuring patient clinical operations remained safe and records secure.",
            "education": "Checking student lesson progress grades and course ratings."
        }
    },
    {
        "text": "If a competitor launches a superior visual layout, what is your response?",
        "options": {
            "frontend": "Refactoring CSS structures to build interactive layout animations.",
            "backend": "Optimizing database speeds so our system remains faster than the competitor.",
            "fullstack": "Rebuilding UI page states and optimizing server data delivery speeds.",
            "data": "Analyzing user behavior data to see if the competitor's layout attracts more users.",
            "devops": "Ensuring deployment configurations support rapid style updates without downtime.",
            "ml": "Tuning recommendation models to deliver better custom results to our users.",
            "business": "Re-evaluating target markets and planning a competitive business campaign.",
            "design": "Redesigning visual layout grids, typography, and page design aesthetics.",
            "healthcare": "Focusing on medical safety and data reliability over cosmetic style updates.",
            "education": "Upgrading learning course visuals to keep lessons engaging and modern."
        }
    },
    {
        "text": "What is your approach to scoping a Minimum Viable Product (MVP)?",
        "options": {
            "frontend": "Building page mockups, responsive CSS styles, and basic user paths.",
            "backend": "Setting up core databases, API routers, and secure login endpoints.",
            "fullstack": "Building a simple fully functioning app containing only the core feature set.",
            "data": "Setting up basic data tables to capture user event logs.",
            "devops": "Configuring a basic staging server and cloud pipeline to deploy changes.",
            "ml": "Training a simple baseline classification model to test prediction logic.",
            "business": "Defining the core business problem, customer target, and revenue goals.",
            "design": "Creating wireframe drawings, layout systems, and page navigation flows.",
            "healthcare": "Ensuring patient safety rules are followed even in early prototype builds.",
            "education": "Defining key learning concepts and structuring the first lesson draft."
        }
    },
    {
        "text": "How do you select which features to build next?",
        "options": {
            "frontend": "Checking which UI improvements are most requested by frontend users.",
            "backend": "Selecting API upgrades that improve server performance and data security.",
            "fullstack": "Prioritizing tasks that solve client issues while keeping database schemas clean.",
            "data": "Analyzing analytics datasets to identify user pattern drop-off areas.",
            "devops": "Selecting backend infrastructure updates that reduce monthly hosting costs.",
            "ml": "Prioritizing model updates that improve prediction accuracy and latency.",
            "business": "Evaluating features based on financial costs, timeline constraints, and ROI.",
            "design": "Selecting layout upgrades based on usability tests and visual design needs.",
            "healthcare": "Focusing on updates that improve medical clinical safety and patient records.",
            "education": "Selecting lesson updates based on tutor feedback and student grade metrics."
        }
    },
    {
        "text": "If a client demands a feature that breaks technical conventions, what do you do?",
        "options": {
            "frontend": "Finding custom CSS and JS code structures to build the layout safely.",
            "backend": "Designing custom database tables and API routing logic to support the request.",
            "fullstack": "Explaining the cost, refactoring the system, and building a workaround.",
            "data": "Writing customized SQL queries to process the unique data request.",
            "devops": "Isolating the unique service in a separate container to protect other servers.",
            "ml": "Tuning a specialized model to handle the unique data configuration request.",
            "business": "Evaluating the customer contract value against developer development hours.",
            "design": "Proposing alternative layout wireframes that satisfy the user intent safely.",
            "healthcare": "Rejecting any feature request that violates patient privacy or clinical compliance.",
            "education": "Adapting learning curriculum formats to support special student requirements."
        }
    },
    {
        "text": "How do you manage software licensing costs?",
        "options": {
            "frontend": "Using open-source frontend style templates and code packages.",
            "backend": "Prioritizing open-source databases (like PostgreSQL) and server frameworks.",
            "fullstack": "Using open-source codebases to avoid paying premium platform royalties.",
            "data": "Evaluating data storage pricing per query and choosing cost-effective options.",
            "devops": "Using open-source container systems and comparing hosting costs.",
            "ml": "Using open-source models and checking licensing rules before training.",
            "business": "Negotiating enterprise software subscription packages and monitoring budgets.",
            "design": "Comparing Figma licenses, font styling royalties, and visual asset costs.",
            "healthcare": "Ensuring all health software licenses comply with medical compliance laws.",
            "education": "Using free educational tools to keep learning software costs low."
        }
    },
    {
        "text": "What is your approach to handling customer support queries?",
        "options": {
            "frontend": "Fixing layout display issues and correcting user interface bugs.",
            "backend": "Debugging API routing errors and correcting backend database glitches.",
            "fullstack": "Helping debug issues that occur across any layer of the app.",
            "data": "Running queries to find out why metrics reports are showing discrepancies.",
            "devops": "Investigating server crash logs and correcting system permissions.",
            "ml": "Analyzing why prediction engines delivered incorrect recommendations.",
            "business": "Managing client relations, evaluating refund queries, and prioritising tickets.",
            "design": "Analyzing visual feedback to simplify page navigational flows.",
            "healthcare": "Responding immediately to patient record issues to maintain medical safety.",
            "education": "Answering student curriculum questions and explaining lesson guides."
        }
    },
    {
        "text": "How do you handle technical debt?",
        "options": {
            "frontend": "Refactoring CSS classes, simplifying component styling, and updating libraries.",
            "backend": "Cleaning database schemas, refactoring controller code, and updating APIs.",
            "fullstack": "Updating outdated dependencies across both client and server repos.",
            "data": "Redesigning legacy databases and archiving outdated data tables.",
            "devops": "Upgrading server OS versions, cleaning docker files, and simplifying cloud setups.",
            "ml": "Retraining model layers on fresh data sets and cleaning pipeline files.",
            "business": "Allocating developer sprint cycles to fix code structure instead of building features.",
            "design": "Consolidating design files, cleaning style guides, and updating template designs.",
            "healthcare": "Ensuring compliance updates are integrated into medical database code.",
            "education": "Reorganizing outdated lesson curriculum plans to match fresh industry standards."
        }
    },
    {
        "text": "What is your approach to product pricing strategies?",
        "options": {
            "frontend": "Designing visually attractive payment pages and price tables.",
            "backend": "Building secure subscription databases, billing APIs, and discount code routes.",
            "fullstack": "Connecting client-side pricing views to server payment gateway APIs.",
            "data": "Analyzing user purchasing data to calculate conversion price points.",
            "devops": "Ensuring backend billing servers remain secure and scale automatically.",
            "ml": "Building models to predict customer price sensitivity based on usage metrics.",
            "business": "Conducting competitor analysis, evaluating profit margins, and setting price tiers.",
            "design": "Designing layout structures that highlight premium tier features clearly.",
            "healthcare": "Structuring insurance data and medical pricing compliance pipelines.",
            "education": "Designing student scholarship packages and low-cost learning options."
        }
    },
    # 10. Human Services and Teaching (91-100)
    {
        "text": "When a student fails to understand a concept, what do you do?",
        "options": {
            "frontend": "Building interactive visual graphics to illustrate the concept clearly.",
            "backend": "Providing clean log outputs and documentation to explain the logic.",
            "fullstack": "Creating step-by-step interactive coding exercises covering all app layers.",
            "data": "Plotting simplified charts that visualize the data correlations clearly.",
            "devops": "Creating simple setup scripts that help students build systems without errors.",
            "ml": "Tuning educational models to adjust learning speeds to match user needs.",
            "business": "Assessing if training costs justify rewriting learning materials.",
            "design": "Designing simple infographics, layouts, and visual study diagrams.",
            "healthcare": "Explaining medical concepts to patient charts using simple terminology.",
            "education": "Adapting pedagogical methods, creating tutorials, and scheduling mentoring sessions."
        }
    },
    {
        "text": "How do you approach tutoring or training someone in coding?",
        "options": {
            "frontend": "Teaching page design layouts, CSS transitions, and HTML component creation.",
            "backend": "Teaching server-side logic, database query writing, and API routing setups.",
            "fullstack": "Guiding them to build complete web applications from database to UI views.",
            "data": "Teaching SQL commands, database design, and data visualization tools.",
            "devops": "Teaching server commands, Docker configs, and continuous integration setups.",
            "ml": "Teaching neural network math, model training scripts, and evaluation rules.",
            "business": "Explaining Agile project management, customer reviews, and product strategy.",
            "design": "Teaching UI layouts, Figma vector drawing, and usability principles.",
            "healthcare": "Teaching clinic staff how to input patient symptoms into databases.",
            "education": "Structuring lesson modules, planning curriculum guides, and offering mentoring."
        }
    },
    {
        "text": "What type of student feedback makes you feel most accomplished?",
        "options": {
            "frontend": "Students expressing excitement at building their first beautiful web page layout.",
            "backend": "Students understanding how servers communicate with databases via APIs.",
            "fullstack": "Students successfully launching their first full-stack application online.",
            "data": "Students successfully writing complex SQL queries to clean datasets.",
            "devops": "Students mastering terminal commands and deploying server containers.",
            "ml": "Students understanding the math behind machine learning prediction models.",
            "business": "Students pitching a product business model canvas to startup investors.",
            "design": "Students creating professional, clean, and legibly formatted UI wireframes.",
            "healthcare": "Nursing students learning how to navigate medical charts quickly.",
            "education": "Students landing their first job using your curriculum and mentoring."
        }
    },
    {
        "text": "How do you evaluate student learning progress?",
        "options": {
            "frontend": "Reviewing their website designs and CSS layouts for code neatness.",
            "backend": "Checking their database schemas and testing their backend API routes.",
            "fullstack": "Testing the features and database integrations of their completed web apps.",
            "data": "Reviewing their data analytics reports and testing their SQL logic.",
            "devops": "Checking their docker build scripts and staging deployment servers.",
            "ml": "Evaluating the prediction accuracy scores of models they trained.",
            "business": "Reviewing their product specification documents and business slide decks.",
            "design": "Reviewing their design component libraries and prototype layout usability.",
            "healthcare": "Verifying their clinical documentation meets safety compliance codes.",
            "education": "Setting up quizzes, reviewing homework projects, and tracking lesson metrics."
        }
    },
    {
        "text": "If a student finds course material too difficult, what do you change?",
        "options": {
            "frontend": "Designing more visual styling exercises and coding templates.",
            "backend": "Creating simpler database exercises and writing clearer API code comments.",
            "fullstack": "Focusing them on basic HTML forms and simple database CRUD logic first.",
            "data": "Providing pre-cleaned datasets to simplify data analysis projects.",
            "devops": "Providing pre-configured docker setups so they don't get stuck on command steps.",
            "ml": "Explaining neural concepts using analogies instead of complex calculus math.",
            "business": "Focusing their studies on marketing examples instead of technical budgets.",
            "design": "Simplifying the design system tasks and offering basic layout guidelines.",
            "healthcare": "Focusing medical informatics studies on patient records basics first.",
            "education": "Breaking complex lessons into sub-modules, and adding tutorial guidance."
        }
    },
    {
        "text": "How do you select which educational materials to purchase?",
        "options": {
            "frontend": "Purchasing premium CSS design tutorial books and styling resources.",
            "backend": "Acquiring access to advanced database courses and secure server API tutorials.",
            "fullstack": "Subscribing to full-stack framework video tutorials and developer bundles.",
            "data": "Acquiring datasets, SQL training platforms, and BI data visualization licenses.",
            "devops": "Subscribing to cloud sandbox servers and terminal command tutorials.",
            "ml": "Acquiring access to GPU cloud servers and machine learning research articles.",
            "business": "Purchasing agile management guides and corporate product strategy books.",
            "design": "Acquiring Figma team licenses, font styling libraries, and design style books.",
            "healthcare": "Purchasing medical standards guidelines and healthcare compliance manuals.",
            "education": "Buying curriculum textbooks, instructional design software, and mentoring guides."
        }
    },
    {
        "text": "What is the primary role of a mentor in professional training?",
        "options": {
            "frontend": "Guiding students to write clean visual layouts and responsive component styles.",
            "backend": "Teaching students to secure API routes and design efficient database tables.",
            "fullstack": "Teaching students to integrate all layers of an application successfully.",
            "data": "Helping students draw insights from datasets and write clean SQL queries.",
            "devops": "Teaching students to configure clouds and write deploy scripts safely.",
            "ml": "Guiding students to train, evaluate, and tune prediction models.",
            "business": "Helping students define product MVPs, budgets, and business pitches.",
            "design": "Guiding students to build cohesive design styles and clear UI wireframes.",
            "healthcare": "Teaching clinic teams to log patient health metrics with safety compliance.",
            "education": "Offering career guidance, clarifying concepts, and designing lesson paths."
        }
    },
    {
        "text": "How do you structure classroom workshops?",
        "options": {
            "frontend": "Structuring workshops around building and styling interactive web components.",
            "backend": "Designing sessions to code server endpoints and connect backend databases.",
            "fullstack": "Guiding groups to build a simple web service from scratch during the session.",
            "data": "Focusing sessions on loading datasets, cleaning values, and plotting charts.",
            "devops": "Leading exercises to run docker container commands and verify setups.",
            "ml": "Structuring workshops to train and score simple classification models.",
            "business": "Guiding teams to draft product briefs and pitch startup ideas.",
            "design": "Structuring workshops to draw Figma wireframes and run layout tests.",
            "healthcare": "Leading clinical simulation tests to practice recording patient symptom data.",
            "education": "Defining lesson outcomes, organizing group tasks, and scheduling review points."
        }
    },
    {
        "text": "What is your strategy when teaching coding to complete beginners?",
        "options": {
            "frontend": "Beginning with simple HTML tags, styling colors, and flexbox UI layout rules.",
            "backend": "Beginning with basic variables, if-statements, and reading database tables.",
            "fullstack": "Showing them how button clicks trigger database updates in simple apps.",
            "data": "Beginning with simple SQL filters and plotting basic spreadsheet charts.",
            "devops": "Beginning with simple folder commands and basic server hosting concepts.",
            "ml": "Beginning with basic linear datasets and plotting correlation lines.",
            "business": "Explaining why companies build software and what product value means.",
            "design": "Showing them how to structure layout grids and align visual components.",
            "healthcare": "Explaining the importance of clear medical record coding for patient safety.",
            "education": "Using visual block coding, simple exercises, and encouraging tutoring check-ins."
        }
    },
    {
        "text": "How do you handle grading class assignments?",
        "options": {
            "frontend": "Checking layout designs for visual styling appeal and screen responsiveness.",
            "backend": "Testing API endpoint inputs and checking database transaction code.",
            "fullstack": "Evaluating full features to ensure database writes show up correctly on pages.",
            "data": "Checking SQL script execution speeds and the accuracy of output data.",
            "devops": "Verifying that container configurations build and deploy without errors.",
            "ml": "Evaluating model prediction scores against validation target metrics.",
            "business": "Checking if product plans align with market research and business goals.",
            "design": "Evaluating components for usability contrast, font hierarchy, and grid layout.",
            "healthcare": "Verifying that clinical mockups follow medical privacy and HIPAA rules.",
            "education": "Applying standardized grading matrices and writing constructive student feedback."
        }
    },
    # 11. Security, Risk and Compliance (101-110)
    {
        "text": "What is your first priority when audit season arrives?",
        "options": {
            "frontend": "Checking frontend dependencies for security alerts and fixing libraries.",
            "backend": "Safety-checking API access scopes, token hashing, and route security keys.",
            "fullstack": "Reviewing user session data handling from client screens to database files.",
            "data": "Auditing database access permissions and tracking row modifications.",
            "devops": "Reviewing AWS IAM permissions, server firewall rules, and VPC logs.",
            "ml": "Auditing training sets for data bias and checking model evaluation logs.",
            "business": "Reviewing corporate legal policies, insurance coverages, and financial balances.",
            "design": "Auditing design layouts for accessibility contrast and keyboard navigation.",
            "healthcare": "Verifying complete compliance audits with HIPAA patient record privacy rules.",
            "education": "Checking student academic records for FERPA educational compliance."
        }
    },
    {
        "text": "How do you design login systems for security?",
        "options": {
            "frontend": "Securing session storage tokens and designing clear error validation warnings.",
            "backend": "Implementing secure password hashing (BCrypt), JWT scopes, and api rate limits.",
            "fullstack": "Building multi-factor auth forms and verifying verification codes in the API.",
            "data": "Tracking login trends to flag abnormal account access attempts.",
            "devops": "Setting up single-sign-on (SSO) links, firewalls, and securing API routers.",
            "ml": "Training model networks to detect robotic login behavior patterns.",
            "business": "Assessing risk rules to balance security against customer registration conversion.",
            "design": "Designing clear authorization screens and help guides for login issues.",
            "healthcare": "Applying multi-signature biometric checks for medical diagnostic data access.",
            "education": "Configuring simple school SSO access to let students login securely."
        }
    },
    {
        "text": "What is your approach to handling customer user delete requests (GDPR)?",
        "options": {
            "frontend": "Designing user-friendly deletion confirmation screens and account toggles.",
            "backend": "Writing secure SQL queries to scrub user data fields from database tables.",
            "fullstack": "Connecting user settings forms to database delete endpoints across API gates.",
            "data": "Running batch scripts to scrub user identifiers from data warehouse backups.",
            "devops": "Ensuring server storage backups are configured to process data deletes safely.",
            "ml": "Removing user parameters from training sets and retraining models.",
            "business": "Consulting legal experts to verify privacy policy updates are correct.",
            "design": "Designing clear explanations to help users understand what data is deleted.",
            "healthcare": "Retaining patient medical charts as required by medical law while securing them.",
            "education": "Archiving student grade records securely while purging personal profile details."
        }
    },
    {
        "text": "How do you secure server-to-server communications?",
        "options": {
            "frontend": "Ensuring client calls use HTTPS routes and verify API key headers.",
            "backend": "Using secure gRPC routes, server API keys, and verifying token origins.",
            "fullstack": "Designing secure client-server architectures with encrypted data payloads.",
            "data": "Enforcing SSL credentials for database connections and read-replicate links.",
            "devops": "Configuring VPN networks, VPC peering routes, and private IP structures.",
            "ml": "Encrypting model training data pipelines across cloud processing servers.",
            "business": "Negotiating security agreements with corporate partners and providers.",
            "design": "Designing UI warnings for users when network connections are unsecured.",
            "healthcare": "Enforcing medical-standard encryption tunnels (like HL7 VPNs) for health data.",
            "education": "Securing data routes between school portals and student progress apps."
        }
    },
    {
        "text": "What is your response to a potential data leak alert?",
        "options": {
            "frontend": "Checking frontend scripts for memory leaks or unsecured cookie exposures.",
            "backend": "Revoking API token access, rotating secret keys, and checking endpoint logs.",
            "fullstack": "Investigating full-stack system flows to find where data leak failures occurred.",
            "data": "Checking database tables to see which user records were accessed.",
            "devops": "Locking down cloud network routes, shutting ports, and inspecting VPC traffic logs.",
            "ml": "Checking if models are leaks training features in prediction outputs.",
            "business": "Drafting emergency incident statements and communicating with legal advisors.",
            "design": "Designing client notification layouts explaining the security warning clearly.",
            "healthcare": "Activating emergency hospital safety plans to secure clinician accounts.",
            "education": "Notifying parents and students of security risks on student portals."
        }
    },
    {
        "text": "How do you manage software updates for security?",
        "options": {
            "frontend": "Checking npm packages for vulnerability notices and updating CSS/JS files.",
            "backend": "Updating server dependencies, database frameworks, and API components.",
            "fullstack": "Running code test suites to verify security updates do not break features.",
            "data": "Updating database server versions and verifying query schema compatibility.",
            "devops": "Automating container updates and running vulnerability scanner tools.",
            "ml": "Upgrading ML libraries and verifying neural layer compute drivers.",
            "business": "Scheduling software update timelines to avoid business sale downtime.",
            "design": "Verifying that design component updates match visual library layouts.",
            "healthcare": "Testing updates in medical test sandboxes to protect clinical software.",
            "education": "Scheduling learning portal updates during school holidays to avoid class breaks."
        }
    },
    {
        "text": "What is your strategy for securing api keys?",
        "options": {
            "frontend": "Avoiding saving API keys in client-side JS bundles and using server proxies.",
            "backend": "Storing keys in environment configurations and validating origins on server calls.",
            "fullstack": "Accessing third-party services via backend routes instead of client files.",
            "data": "Restricting database API credentials to read-only roles when querying tables.",
            "devops": "Using cloud vault key managers and rotating passwords automatically.",
            "ml": "Securing private model endpoint credentials to prevent prediction spam.",
            "business": "Reviewing security compliance rules before acquiring new API keys.",
            "design": "Documenting style access settings to prevent public sharing of visual libraries.",
            "healthcare": "Restricting medical API credentials to verified clinical integration networks.",
            "education": "Restricting student access to API sandbox keys to prevent code misuse."
        }
    },
    {
        "text": "How do you handle software compliance checks?",
        "options": {
            "frontend": "Verifying frontend markup follows W3C accessibility compliance guidelines (WCAG).",
            "backend": "Ensuring database transactions comply with ACID safety rules.",
            "fullstack": "Testing all application paths to verify security compliance criteria are met.",
            "data": "Ensuring datasets are processed according to company data handling rules.",
            "devops": "Setting up compliance monitoring tools to check cloud resource configs.",
            "ml": "Evaluating machine learning outputs to ensure model decisions avoid bias.",
            "business": "Ensuring company systems meet industry compliance certificates (like SOC2).",
            "design": "Testing layout designs for readability and disability accessibility standards.",
            "healthcare": "Ensuring clinical technology follows strict HIPAA and medical device codes.",
            "education": "Verifying educational tools comply with children digital safety acts (COPPA)."
        }
    },
    {
        "text": "What is your approach to system access logs?",
        "options": {
            "frontend": "Logging user interface errors and navigation tracking logs.",
            "backend": "Writing detailed API request log lines and tracking database updates.",
            "fullstack": "Integrating user interface session logs with backend API database logs.",
            "data": "Reviewing access tables to check which metrics reports were generated.",
            "devops": "Configuring centralized logging servers to capture cloud system access events.",
            "ml": "Logging training parameters and model version access history logs.",
            "business": "Reviewing employee access logs to safety-verify business document privacy.",
            "design": "Analyzing click maps and heatmaps to check layout usability paths.",
            "healthcare": "Maintaining strict audit logs showing which doctor read which patient chart.",
            "education": "Tracking tutor access logs on grade evaluation screens for security."
        }
    },
    {
        "text": "How do you evaluate security risks of cloud hosting options?",
        "options": {
            "frontend": "Comparing SSL support and header security settings on client host hosting sites.",
            "backend": "Evaluating database encryption options and API firewall support.",
            "fullstack": "Comparing full-stack cloud providers for deployment ease and security.",
            "data": "Checking database isolation layers and analytic warehouse security features.",
            "devops": "Reviewing cloud provider compliance certs, VPC setups, and IAM tools.",
            "ml": "Checking if cloud ML servers keep model data private from public networks.",
            "business": "Evaluating cloud pricing plans against compliance security guarantees.",
            "design": "Ensuring layout asset hosting sites support encrypted asset transfers.",
            "healthcare": "Choosing healthcare-specialized private cloud servers for patient systems.",
            "education": "Verifying cloud hosts comply with school student privacy requirements."
        }
    },
    # 12. Scaling and Infrastructure (111-120)
    {
        "text": "When server traffic doubles in an hour, how does your system react?",
        "options": {
            "frontend": "Serving static page files from CDN caches to keep browser loading fast.",
            "backend": "Scaling database connection pools and using Redis caching for API data.",
            "fullstack": "Managing state caching on both UI layers and backend server nodes.",
            "data": "Postponing heavy analytical runs to protect core transaction databases.",
            "devops": "Triggering auto-scaling groups to boot new container nodes automatically.",
            "ml": "Tuning model prediction routing to balance requests across GPU nodes.",
            "business": "Monitoring cloud billing alerts and evaluating sales growth values.",
            "design": "Designing layout templates that keep running smoothly on slow connections.",
            "healthcare": "Prioritizing clinician database routes over administrative page updates.",
            "education": "Distributing online exam traffic across backup classroom server links."
        }
    },
    {
        "text": "What is your approach to reducing cloud hosting costs?",
        "options": {
            "frontend": "Optimizing image weights and minifying web files to save bandwidth costs.",
            "backend": "Refactoring API logic code to run with lower memory usage metrics.",
            "fullstack": "Shutting down inactive staging systems and using compact server apps.",
            "data": "Optimizing data storage layouts and scheduling query runs to avoid peak times.",
            "devops": "Configuring auto-scale schedules, cleaning logs, and comparing server sizes.",
            "ml": "Compressing model weights, using CPU models, and cleaning datasets.",
            "business": "Negotiating cloud subscription discounts and setting monthly cost alerts.",
            "design": "Using stock styling libraries and design templates to reduce prototyping costs.",
            "healthcare": "Archiving historical medical databases to cheap, secure cold arrays.",
            "education": "Deactivating student test servers during school holidays to save budgets."
        }
    },
    {
        "text": "How do you distribute content globally to reduce latency?",
        "options": {
            "frontend": "Uploading web build files to global Content Delivery Networks (CDNs).",
            "backend": "Deploying API server replicas in multiple global server zones.",
            "fullstack": "Configuring global hosting platforms to deploy both UI and API layers locally.",
            "data": "Replicating reporting database charts to regional analytics servers.",
            "devops": "Setting up DNS routing, global CDNs, and multi-region database sync.",
            "ml": "Hosting model prediction nodes in datacenters closest to target users.",
            "business": "Analyzing global customer location metrics to plan region launches.",
            "design": "Designing style elements that load instantly from global web fonts.",
            "healthcare": "Ensuring patient charts are hosted within regional legal borders for safety compliance.",
            "education": "Caching educational video assets in CDNs near regional student clusters."
        }
    },
    {
        "text": "What is your strategy for database scaling?",
        "options": {
            "frontend": "Caching database responses in browser state to minimize database calls.",
            "backend": "Implementing read-replicas, write connection pools, and database caching.",
            "fullstack": "Designing features that limit database reads and distribute write loads.",
            "data": "Setting up table partitions, sharding schemas, and building data warehouses.",
            "devops": "Configuring database clustering, storage auto-growth, and server scaling.",
            "ml": "Caching model datasets on fast local SSD storage arrays for training runs.",
            "business": "Budgeting database upgrade budgets based on projected customer numbers.",
            "design": "Ensuring search layouts remain responsive during database scaling lags.",
            "healthcare": "Clustering medical database nodes to ensure patient record availability.",
            "education": "Configuring student database partitions by school year or class groups."
        }
    },
    {
        "text": "How do you manage software release updates on servers?",
        "options": {
            "frontend": "Verifying that client browser cache clears after style updates.",
            "backend": "Running database migrations and checking API route health post-update.",
            "fullstack": "Testing the deployment on staging before rolling changes to production.",
            "data": "Updating database schema versions and verifying analytical report exports.",
            "devops": "Using blue-green deployments, canary testing, and automated rollbacks.",
            "ml": "A/B testing model prediction outputs before switching server routes.",
            "business": "Communicating release features to clients and coordinate sales plans.",
            "design": "Verifying layout aesthetic consistency across release versions.",
            "healthcare": "Scheduling updates during clinical quiet hours to protect patient care.",
            "education": "Updating lesson guides and tutorials to match the release updates."
        }
    },
    {
        "text": "What tools do you use to monitor application health?",
        "options": {
            "frontend": "Browser console tracking, client speed analytics, and error tracking tools.",
            "backend": "API log monitors, server health endpoints, and database connection alerts.",
            "fullstack": "Combined client-server dashboard alerts showing overall performance.",
            "data": "Database query statistics, transaction queues, and data check scripts.",
            "devops": "Datadog, Prometheus, Grafana, and cloud resource monitors.",
            "ml": "Model drift tracking dashboards and prediction distribution metrics.",
            "business": "Google Analytics, Mixpanel, and customer support ticket trends.",
            "design": "User session recorders, click heatmaps, and layout exit surveys.",
            "healthcare": "Hospital clinic system status boards and safety alarm networks.",
            "education": "Student active count graphs and quiz performance averages."
        }
    },
    {
        "text": "How do you handle background processing tasks?",
        "options": {
            "frontend": "Using Web Workers to run heavy visual calculations without blocking pages.",
            "backend": "Setting up background worker queues (like Hangfire) to run server logic.",
            "fullstack": "Creating background tasks that update database states and notify the UI.",
            "data": "Scheduling nightly database cleanups and data processing scripts.",
            "devops": "Configuring server cron jobs and message broker event handlers.",
            "ml": "Scheduling offline batch prediction scripts and model training pipelines.",
            "business": "Automating weekly sales reports and emailing updates to team leads.",
            "design": "Designing clear loading animations for tasks that process in the background.",
            "healthcare": "Processing medical audit reviews in background queues to keep vitals live.",
            "education": "Scheduling weekly homework evaluation emails and student progress updates."
        }
    },
    {
        "text": "What is your approach to infrastructure documentation?",
        "options": {
            "frontend": "Documenting deployment steps for visual assets and styling builds.",
            "backend": "Mapping server database connections, API configurations, and routes.",
            "fullstack": "Publishing complete system architecture diagrams covering all app layers.",
            "data": "Documenting database data maps, warehouses, and ETL data paths.",
            "devops": "Writing Terraform scripts, network map files, and server recovery guides.",
            "ml": "Mapping model serving endpoints, model servers, and GPU configurations.",
            "business": "Tracking infrastructure cost sheets, software licenses, and provider contracts.",
            "design": "Documenting asset export rules and graphic file storage folders.",
            "healthcare": "Mapping clinical systems connections to meet safety compliance rules.",
            "education": "Writing installation guides to help tutors set up training databases."
        }
    },
    {
        "text": "How do you select your hosting provider?",
        "options": {
            "frontend": "Comparing Vercel, Netlify, or AWS Amplify for frontend hosting ease.",
            "backend": "Evaluating Heroku, AWS EC2, or Azure App Services for server deployment.",
            "fullstack": "Selecting providers that support rapid full-stack container deploys.",
            "data": "Comparing Google Cloud, AWS, or Snowflake for analytical database power.",
            "devops": "Evaluating AWS, GCP, or Azure for network tools, IAM, and infrastructure.",
            "ml": "Choosing hosting providers that offer cost-effective GPU computation instances.",
            "business": "Comparing hosting SLAs, compliance certs, and contract costs.",
            "design": "Ensuring the host platform supports web preview links for visual mockups.",
            "healthcare": "Selecting HIPAA-compliant medical-grade private cloud hosting systems.",
            "education": "Choosing low-cost hosting systems that support learning portals."
        }
    },
    {
        "text": "What is your approach to system recovery testing (chaos engineering)?",
        "options": {
            "frontend": "Simulating network drops in browsers to check local cache fallbacks.",
            "backend": "Testing server logic recovery when database connections drop.",
            "fullstack": "Verifying that client apps recover elegantly when API services crash.",
            "data": "Simulating database failures to verify automated data backups recover safely.",
            "devops": "Shutting down servers dynamically to verify cloud auto-heal systems work.",
            "ml": "Simulating prediction service drops to verify fallbacks to default outputs.",
            "business": "Evaluating business risk recovery plans and scheduling crisis exercises.",
            "design": "Designing clear system error screens to inform users when recovery is running.",
            "healthcare": "Testing clinical failovers to ensure medical alerts remain active.",
            "education": "Testing school portal backups to ensure student progress data is saved."
        }
    },
    # 13. Learning and Skill Development (121-130)
    {
        "text": "What technical topic are you currently studying?",
        "options": {
            "frontend": "Advanced Tailwind styles, React rendering states, or Next.js layout structures.",
            "backend": "PostgreSQL indexing, microservices design patterns, or secure API logic.",
            "fullstack": "Monorepo strategies, full-stack framework features, and deployment tools.",
            "data": "Snowflake data warehouses, Spark scripts, and analytical query tuning.",
            "devops": "Kubernetes setups, Terraform templates, and AWS cloud networking.",
            "ml": "Deep learning models, natural language transformers, and neural systems.",
            "business": "Agile scrum methods, product management strategy, and user growth analytics.",
            "design": "Typography systems, Figma visual mockups, and interaction usability guidelines.",
            "healthcare": "Health informatics compliance, medical terminology, and patient safety rules.",
            "education": "Pedagogical design, student mentoring, and training course structures."
        }
    },
    {
        "text": "How do you prefer to learn a new programming language?",
        "options": {
            "frontend": "Building interactive page layouts, styling elements, and using CSS hooks.",
            "backend": "Writing server logic scripts, API routes, and connecting database queries.",
            "fullstack": "Building a simple CRUD application that connects database to frontend views.",
            "data": "Writing scripts to process files, filter rows, and analyze datasets.",
            "devops": "Writing build scripts, automating folders, and calling API commands.",
            "ml": "Implementing math formulas, arrays, and training simple models.",
            "business": "Studying why companies use that language and how it affects hiring costs.",
            "design": "Reviewing visual code editors and template layout features.",
            "healthcare": "Checking if the language supports secure health compliance libraries.",
            "education": "Reading structured guides, doing quizzes, and taking tutorial classes."
        }
    },
    {
        "text": "What type of online course appeals to you most?",
        "options": {
            "frontend": "Visual CSS styling tricks, responsive component structures, and JS libraries.",
            "backend": "Distributed system architectures, secure database queries, and backend API routes.",
            "fullstack": "Complete guides to building and launching SaaS applications from scratch.",
            "data": "Big data analysis, data warehousing scripts, and SQL dashboard creations.",
            "devops": "Docker containers, Kubernetes setups, and cloud automation pipelines.",
            "ml": "Neural network architectures, deep learning math, and AI model tuning.",
            "business": "Product strategy frameworks, business finance, and growth metrics analysis.",
            "design": "UI/UX wireframing principles, color theory, and high-fidelity mockups.",
            "healthcare": "Clinical systems operations, health laws, and patient data safety.",
            "education": "Curriculum planning, student evaluation methods, and mentoring strategies."
        }
    },
    {
        "text": "What do you do when you get stuck on a difficult code error?",
        "options": {
            "frontend": "Checking browser element logs, styling parameters, and search solutions online.",
            "backend": "Running server debugger tools, checking database tables, and API logs.",
            "fullstack": "Checking the flow from the client input form down to the server logs.",
            "data": "Rewriting the SQL query step-by-step and checking raw dataset columns.",
            "devops": "Reviewing cloud event logs, docker configurations, and network settings.",
            "ml": "Plotting tensor shapes, checking dataset dimensions, and print statements.",
            "business": "Asking developer leads to estimate the cost of debugging versus redesigning.",
            "design": "Consulting layout guidelines and checking Figma mockups for consistency.",
            "healthcare": "Logging the issue in database logs while ensuring patient systems remain safe.",
            "education": "Asking a colleague for help, or writing a tutorial note once solved."
        }
    },
    {
        "text": "Which technical community or forum do you visit most?",
        "options": {
            "frontend": "Frontend developer subreddits, CSS Discord servers, and GitHub repos.",
            "backend": "StackOverflow database boards, server-side tech forums, and API design blogs.",
            "fullstack": "Tech startup forums, SaaS development groups, and coding channels.",
            "data": "Data engineering communities, SQL forums, and Kaggle boards.",
            "devops": "DevOps Reddit, Cloud provider forums, and Terraform sharing channels.",
            "ml": "Machine learning subreddits, AI research groups, and model sharing sites.",
            "business": "Product management forums, business strategy blogs, and growth boards.",
            "design": "Design sharing sites, UX review boards, and design style communities.",
            "healthcare": "Health technology boards and clinical database compliance groups.",
            "education": "Teacher mentoring boards, pedagogy forums, and course design communities."
        }
    },
    {
        "text": "How do you feel about learning computer science theory (algorithms)?",
        "options": {
            "frontend": "Find it useful for page search tools, but prioritize layout responsiveness.",
            "backend": "Value it highly for server speed, query optimization, and memory use.",
            "fullstack": "Appreciate it for structuring complex full-stack features efficiently.",
            "data": "Value sorting and tree search rules to optimize database operations.",
            "devops": "Focus on network routing algorithms and server load balancer setups.",
            "ml": "Rely on matrix math and optimization algorithms to train models.",
            "business": "Value algorithmic logic to optimize pricing and financial budgeting.",
            "design": "Prioritize visual layout balance and navigation logic over math code.",
            "healthcare": "Ensure algorithm operations follow clinical safety check regulations.",
            "education": "Enjoy translating logic rules into simple learning blocks for students."
        }
    },
    {
        "text": "What type of career growth workshop would you attend?",
        "options": {
            "frontend": "Building modern interactive page components and layout styles.",
            "backend": "Designing secure backend microservices and indexing databases.",
            "fullstack": "Rapidly building and launching online SaaS software independently.",
            "data": "Building streaming data warehouses and big data analysis pipelines.",
            "devops": "Orchestrating server containers and automating cloud setup scripts.",
            "ml": "Tuning generative models, NLP networks, and AI architectures.",
            "business": "Developing product roadmaps, business canvas models, and sales growth.",
            "design": "Creating scalable UI design libraries and running UX usability reviews.",
            "healthcare": "Managing medical records compliance and safety regulations.",
            "education": "Developing student training systems, mentoring programs, and lesson paths."
        }
    },
    {
        "text": "How do you share your knowledge with your colleagues?",
        "options": {
            "frontend": "Sharing UI component templates, styling tips, and front-end guides.",
            "backend": "Writing API swagger files, SQL tips, and documenting database schemas.",
            "fullstack": "Doing live coding sessions to show how features are built end-to-end.",
            "data": "Building data dashboard views to help others analyze business metrics.",
            "devops": "Writing simple deploy scripts to help coders build staging servers.",
            "ml": "Presenting accuracy metrics and explaining how prediction models work.",
            "business": "Sharing product briefs, sales statistics, and business strategy goals.",
            "design": "Sharing visual layouts, Figma templates, and style documentation.",
            "healthcare": "Training clinical teams to enter symptoms safely into database records.",
            "education": "Hosting mentoring sessions, writing lesson tutorials, and tutoring others."
        }
    },
    {
        "text": "What is your main criteria when choosing a tech stack for a side project?",
        "options": {
            "frontend": "How easily the stack handles responsive layouts, colors, and UI state.",
            "backend": "How robust the database links, server security, and API speeds are.",
            "fullstack": "How fast you can build and launch the entire project from database to page UI.",
            "data": "How easily it handles loading datasets, running SQL queries, and plotting charts.",
            "devops": "How simple it is to host the server in a container and deploy to clouds.",
            "ml": "Whether it supports machine learning models and training calculations.",
            "business": "Whether the tech stack fits current market demand and commercial value.",
            "design": "Whether it provides clean design components and layout templates.",
            "healthcare": "Whether the hosting complies with patient privacy and health security laws.",
            "education": "Whether the coding environment is easy for beginner students to understand."
        }
    },
    {
        "text": "How do you approach learning from failure?",
        "options": {
            "frontend": "Refining CSS styles and debugging visual errors to improve rendering layouts.",
            "backend": "Refactoring database schemas and securing server API endpoints.",
            "fullstack": "Reviewing full-stack integrations to prevent client-server connection drops.",
            "data": "Rerunning data scripts, validating values, and correcting query calculations.",
            "devops": "Reviewing server crash logs, fixing scripts, and updating backup rules.",
            "ml": "Retraining model layers on fresh data sets to correct prediction bias.",
            "business": "Evaluating customer feedback data to pivot product business strategies.",
            "design": "Redesigning visual layouts based on user interface usability tests.",
            "healthcare": "Logging clinical system bugs and updating medical check protocols.",
            "education": "Updating lesson manuals to clarify concepts that students struggled with."
        }
    },
    # 14. Creative Expression and Innovation (131-140)
    {
        "text": "If you were to design a game, what part would you code?",
        "options": {
            "frontend": "The player interface, menus, health bars, and visual character styling.",
            "backend": "The multiplayer server database, user logins, and saving player scores.",
            "fullstack": "The client-side game page and the server matchmaking APIs.",
            "data": "Analyzing player metrics, drop-off levels, and in-game shop items data.",
            "devops": "Hosting game servers, configuring latency networks, and auto-scale rules.",
            "ml": "Developing smart NPC behavior models and predictive challenge paths.",
            "business": "Planning the monetization model, marketing plans, and startup budget.",
            "design": "Drawing game environments, interface wireframes, and menu styles.",
            "healthcare": "Building patient physical therapy games that log healing statistics.",
            "education": "Designing educational challenges that teach lessons while students play."
        }
    },
    {
        "text": "What type of creative project inspires you most?",
        "options": {
            "frontend": "Interactive websites containing rich styles, responsive layouts, and animations.",
            "backend": "Secure, fast APIs that connect complex database tables together.",
            "fullstack": "Building functional web platforms that serve user accounts from scratch.",
            "data": "Infographics and visual charts showing fascinating data patterns.",
            "devops": "Fully automated server systems that handle deployments without manual steps.",
            "ml": "Generative systems that write text, paint graphics, or predict paths.",
            "business": "Launching a startup, pitching plans, and watching active user growth.",
            "design": "Designing layout systems, typography styles, and aesthetic graphics.",
            "healthcare": "Designing digital health guides that help patients manage symptoms.",
            "education": "Developing mentoring courses, lesson plans, and training tutorials."
        }
    },
    {
        "text": "When you have a new idea, what is your first step?",
        "options": {
            "frontend": "Coding a quick visual mockup to test page layouts and colors.",
            "backend": "Designing a database table schema and testing server API routes.",
            "fullstack": "Coding a basic end-to-end prototype app to verify utility.",
            "data": "Looking for public datasets to verify if my hypothesis is true.",
            "devops": "Setting up a staging host instance to test server configurations.",
            "ml": "Tuning a baseline model to check prediction possibilities on sample data.",
            "business": "Scoping a business model canvas and analyzing competitor companies.",
            "design": "Sketching visual wireframes, user journeys, and layout designs.",
            "healthcare": "Consulting health compliance guidelines to safety-check the idea.",
            "education": "Drafting educational outcomes and outlining lesson pathways."
        }
    },
    {
        "text": "How do you approach feature prototyping?",
        "options": {
            "frontend": "Using mock styling frameworks to quickly render page button styles and layouts.",
            "backend": "Writing mock server controllers and hardcoded JSON response endpoints.",
            "fullstack": "Building a simple client-server application showing the core feature flow.",
            "data": "Running quick SQL aggregate reports to check if the database holds the metrics.",
            "devops": "Using local container environments to test system setups quickly.",
            "ml": "Testing pre-trained models on sample datasets to check prediction outputs.",
            "business": "Defining product requirements and presenting prototype ideas to clients.",
            "design": "Drawing high-fidelity layouts in Figma and linking frames into click prototypes.",
            "healthcare": "Simulating clinic setups to test software safety compliance.",
            "education": "Creating draft lessons and teaching mock classes to get student feedback."
        }
    },
    {
        "text": "What type of innovation excites you most in software?",
        "options": {
            "frontend": "Modern user interface frameworks that render responsive styles instantly.",
            "backend": "High-concurrency databases that process database entries in microseconds.",
            "fullstack": "Unified frameworks that make building web applications effortless.",
            "data": "Big data analytics tools that query millions of rows in seconds.",
            "devops": "Serverless hosting networks that scale server nodes without configurations.",
            "ml": "AI architectures that learn from data and automate complex choices.",
            "business": "New digital business models that disrupt global consumer markets.",
            "design": "State-of-the-art layout aesthetics and usability interactions.",
            "healthcare": "Health tech integrations that track vitals and save lives in real-time.",
            "education": "Adaptive educational portals that customize lessons to student grades."
        }
    },
    {
        "text": "If you could invent a tool, what would it do?",
        "options": {
            "frontend": "Instantly convert design layout frames into clean, modular CSS/JS styles.",
            "backend": "Automatically secure API paths and build optimized database indexes.",
            "fullstack": "Generate complete database-backed web apps from a simple text description.",
            "data": "Auto-clean messy dataset tables and draw beautiful data analytics charts.",
            "devops": "Automatically setup cloud configurations and deploy container servers securely.",
            "ml": "Auto-train prediction models with perfect accuracy scores on raw files.",
            "business": "Predict target market growth rates and optimize sales campaigns.",
            "design": "Generate beautiful design systems, layouts, and typography palettes automatically.",
            "healthcare": "Track patient clinical vitals and predict medical emergencies instantly.",
            "education": "Build tailored courses, lessons, and provide tutoring to students automatically."
        }
    },
    {
        "text": "What is your preference regarding software styling?",
        "options": {
            "frontend": "Strongly prefer detailed control over CSS variables, styling layouts, and colors.",
            "backend": "Focus on backend data models, ignoring styling details entirely.",
            "fullstack": "Using structured design kits to style complete web application layouts quickly.",
            "data": "Using clean, structured visual styles on data reporting graphs.",
            "devops": "Formatting command-line logs and terminal screens for readability.",
            "ml": "Formatting visualization metrics charts to present model outcomes clearly.",
            "business": "Ensuring corporate presentation slides match company branding guidelines.",
            "design": "Designing comprehensive brand books, color boards, and font hierarchy keys.",
            "healthcare": "Enforcing safety-compliant visual styles to make patient medical data clear.",
            "education": "Using colorful, motivating lesson styles to keep students engaged."
        }
    },
    {
        "text": "How do you choose your project names?",
        "options": {
            "frontend": "Choosing catchy names that look visually appealing in website headers.",
            "backend": "Choosing names that clearly describe backend systems and databases.",
            "fullstack": "Choosing simple names that reflect the overall product value.",
            "data": "Choosing names that describe the database tables or analytical topics.",
            "devops": "Choosing names that match server naming rules and domain lists.",
            "ml": "Choosing names that reflect the target model prediction function.",
            "business": "Choosing brand names that attract customers and register domain trademarks.",
            "design": "Choosing names that inspire design concepts and graphic styles.",
            "healthcare": "Choosing names that match clinical research codes or medical terms.",
            "education": "Choosing clear, inspiring names that describe the training course topic."
        }
    },
    {
        "text": "What type of technical presentation appeals to you most?",
        "options": {
            "frontend": "Demos showing off web styling tricks, transitions, and user interfaces.",
            "backend": "Deep dives explaining database index setups, API loops, and concurrency.",
            "fullstack": "Walkthroughs showing a startup app built from scratch in under an hour.",
            "data": "Case studies showing how business metrics data saved company budgets.",
            "devops": "Live deployments showing cloud infrastructure built using shell command scripts.",
            "ml": "Visual slides detailing neural network architectures and model evaluations.",
            "business": "Pitch decks highlighting market growth metrics, pricing tiers, and ROI.",
            "design": "Design system reviews explaining UI grids, font sizes, and layout templates.",
            "healthcare": "Research reports showing how medical software improved clinician safety.",
            "education": "Teaching strategies explaining course design, student grades, and mentoring."
        }
    },
    {
        "text": "How do you structure side project folders?",
        "options": {
            "frontend": "Grouping by CSS themes, frontend components, and styling assets.",
            "backend": "Structuring folder trees into controllers, database configurations, and route models.",
            "fullstack": "Isolating folders into client-side views and server-side logic layers.",
            "data": "Structuring folders into ETL clean scripts, dataset files, and reports.",
            "devops": "Grouping folders into docker setups, server scripts, and CI/CD templates.",
            "ml": "Organizing folders by data processors, trained model weights, and scripts.",
            "business": "Structuring company files by product specs, budgets, and marketing plans.",
            "design": "Structuring layout asset folders, logo formats, and design system drafts.",
            "healthcare": "Isolating patient clinical databases from administrative document folders.",
            "education": "Structuring lesson modules, training manuals, and student quiz keys."
        }
    },
    # 15. Client and Stakeholder Interactions (141-150)
    {
        "text": "When a client complains that a page looks ugly, what is your direct fix?",
        "options": {
            "frontend": "Adjusting CSS padding variables, color contrast, and layout alignments.",
            "backend": "We refer the client to the frontend team since we write backend database logic.",
            "fullstack": "Polishing frontend styles while keeping API server endpoints running smoothly.",
            "data": "Redesigning reporting dashboard graphics to present metrics more cleanly.",
            "devops": "Ensuring the styling asset files load instantly via CDN servers.",
            "ml": "Adjusting data visualization layouts in model prediction presentations.",
            "business": "Assessing if customer aesthetic complaints affect product sales metrics.",
            "design": "Revisiting wireframes, selecting fresh color boards, and polishing page UI.",
            "healthcare": "Ensuring clinical safety labels remain visible, prioritising safety over beauty.",
            "education": "Updating educational worksheet designs to make lessons appealing to students."
        }
    },
    {
        "text": "How do you present project progress to clients?",
        "options": {
            "frontend": "Showing live web pages, interactive styling components, and layout flows.",
            "backend": "Presenting working API swagger routes and explaining database speed.",
            "fullstack": "Showing the client a fully working web demo with form actions saving to databases.",
            "data": "Presenting analytical dashboards showing business metrics and trends.",
            "devops": "Showing staging links and explaining server reliability using uptime graphs.",
            "ml": "Presenting prediction outputs, model scores, and AI recommendations.",
            "business": "Presenting timelines, budget spent, user growth metrics, and revenue projections.",
            "design": "Sharing high-fidelity Figma prototypes, design kits, and user journeys.",
            "healthcare": "Presenting medical safety checks, compliance files, and patient systems status.",
            "education": "Presenting student progress averages, lesson outlines, and mentoring guides."
        }
    },
    {
        "text": "What do you do when a customer requests a refund because the system was down?",
        "options": {
            "frontend": "Designing clear system maintenance warnings to prevent complaints.",
            "backend": "Debugging server transaction files to verify the billing downtime logs.",
            "fullstack": "Verifying transaction databases and checking client session states.",
            "data": "Running database analysis reports to calculate total active users affected.",
            "devops": "Investigating load balancer logs, fixing servers, and scheduling recovery.",
            "ml": "Checking if model servers crashed during prediction request traffic.",
            "business": "Reviewing client refund policies, financial balances, and issuing refunds.",
            "design": "Designing clear billing status layout screens for customer assistance.",
            "healthcare": "Responding immediately to verify that no critical patient record data was lost.",
            "education": "Offering students extra learning time and rescheduling training events."
        }
    },
    {
        "text": "How do you gather requirements for a client project?",
        "options": {
            "frontend": "Asking what visual layouts, responsive styles, and navigation paths they prefer.",
            "backend": "Asking what database systems, API payloads, and security levels they require.",
            "fullstack": "Scoping out all features needed across both database and client web layers.",
            "data": "Asking what datasets they own and what analytical reports they want to generate.",
            "devops": "Asking about target user volumes, cloud hosts, and continuous integration paths.",
            "ml": "Asking what prediction choices they want to automate using machine learning models.",
            "business": "Conducting stakeholder interviews, budgeting, and scoping product requirement documents.",
            "design": "Running user research, wireframing page grids, and drafting brand design guides.",
            "healthcare": "Reviewing medical standard requirements and clinical safety needs.",
            "education": "Defining student target skills, curriculum levels, and training requirements."
        }
    },
    {
        "text": "What is your approach to handling customer survey reviews?",
        "options": {
            "frontend": "Correcting layout bugs and CSS responsive design errors mentioned in reviews.",
            "backend": "Fixing API database query failures and server logic bugs.",
            "fullstack": "Improving features mentioned as confusing across all code layers.",
            "data": "Aggregating survey ratings, running sentiment analyses, and plotting dashboards.",
            "devops": "Optimizing host performance times based on speed complaints.",
            "ml": "Feeding survey text arrays to NLP models to classify user sentiment.",
            "business": "Using survey scores to prioritize future feature sprints and product directions.",
            "design": "Analyzing visual feedback to simplify typography and page navigation layouts.",
            "healthcare": "Reviewing clinical software safety feedback to improve patient recording systems.",
            "education": "Reviewing lesson course reviews to improve training curriculum and tutor checks."
        }
    },
    {
        "text": "How do you define a project budget?",
        "options": {
            "frontend": "Estimating frontend component library costs and custom stylesheet hours.",
            "backend": "Estimating database hosting fees, server license costs, and API coding hours.",
            "fullstack": "Estimating total system development hours across frontend and backend layers.",
            "data": "Estimating database storage queries pricing and BI tool license costs.",
            "devops": "Estimating monthly cloud resource costs, domains, and server pricing plans.",
            "ml": "Estimating GPU server training instance fees and model compute costs.",
            "business": "Creating detailed spreadsheets, calculating sales trends, and tracking team payroll.",
            "design": "Estimating layout styling hours, Figma workspace keys, and asset costs.",
            "healthcare": "Budgeting health audit certifications and medical privacy protection tools.",
            "education": "Estimating lesson curriculum planning hours, student licenses, and tutor payroll."
        }
    },
    {
        "text": "What do you write in a project proposal document?",
        "options": {
            "frontend": "Wireframe layouts, proposed style themes, CSS guidelines, and page features.",
            "backend": "Database models, api endpoint guides, security standards, and servers.",
            "fullstack": "Complete architecture reviews detailing both frontend pages and backend logic.",
            "data": "Data warehousing designs, analytical report lists, and data source mappings.",
            "devops": "Cloud deployment plans, docker container configs, and backup recovery tracks.",
            "ml": "Proposed ML model types, model training plans, and prediction criteria.",
            "business": "Project pricing tiers, market fit validation, sales timelines, and client ROI plans.",
            "design": "Typography books, UI components, color palettes, and UX page mockups.",
            "healthcare": "Compliance safety audits, patient privacy rules, and clinical record plans.",
            "education": "Curriculum course layouts, skill check lists, lesson guides, and tutoring schedules."
        }
    },
    {
        "text": "How do you prepare for client presentations?",
        "options": {
            "frontend": "Testing website layouts and ensuring client visual interfaces work without styling errors.",
            "backend": "Testing API routes and verifying database transactions execute smoothly.",
            "fullstack": "Testing the full web app flow to ensure client actions save database records.",
            "data": "Verifying data report charts and checking that output numbers match SQL tables.",
            "devops": "Ensuring the staging server is online and has resources to run demo workloads.",
            "ml": "Tuning prediction models to ensure AI recommendations make sense.",
            "business": "Rehearsing corporate business slide decks, checking budgets, and pricing tiers.",
            "design": "Polishing visual presentation files, wireframe mockups, and UI style grids.",
            "healthcare": "Verifying medical compliance points and patient safety charts.",
            "education": "Structuring the lesson tutorial presentation and verifying course guide details."
        }
    },
    {
        "text": "What is your focus when building a client portfolio website?",
        "options": {
            "frontend": "Building stunning page animations, layouts, custom styles, and responsiveness.",
            "backend": "Building a simple database to store portfolio articles and secure API update routes.",
            "fullstack": "Developing a custom backend CMS page and connecting it to the visual portfolio UI.",
            "data": "Adding database tables to track user click events and portfolio traffic.",
            "devops": "Setting up quick CDN hosting paths and custom domains for the portfolio site.",
            "ml": "Integrating a simple recommendation engine to suggest portfolio items.",
            "business": "Ensuring the portfolio clearly highlights user acquisition values and client testimonials.",
            "design": "Defining style typography, drawing clean mockups, and visual layout branding.",
            "healthcare": "Ensuring any health studies shared in the portfolio maintain patient anonymity.",
            "education": "Structuring the portfolio to double as an educational student reference resource."
        }
    },
    {
        "text": "How do you explain system downtime to a client?",
        "options": {
            "frontend": "Explaining that the page layout didn't render due to styling file delivery failures.",
            "backend": "Explaining that backend database connection pools or API servers encountered crashes.",
            "fullstack": "Explaining the technical failure across system layers and how we corrected the connection.",
            "data": "Explaining that analytics data warehouse scripts encountered processing errors.",
            "devops": "Explaining cloud infrastructure network drops and showing recovery actions taken.",
            "ml": "Explaining that machine learning servers encountered inference latency bottlenecks.",
            "business": "Detailing business backup steps and offering credit compensations.",
            "design": "Designing layout notices to explain system fixes to clients nicely.",
            "healthcare": "Reassuring clinicians that safety failovers worked to protect patient charts.",
            "education": "Rescheduling school training schedules and extending homework submission deadlines."
        }
    }
]

# Ensure we have exactly 150 questions
assert len(quiz_questions_source) == 150, f"Expected 150 questions, got {len(quiz_questions_source)}"

# Generate final structured questions
questions = []
for q_idx, q_source in enumerate(quiz_questions_source):
    q_id = q_idx + 1
    
    # Randomly pick 4 domains for this question
    chosen_domains = random.sample(doms, 4)
    options = []
    
    for i, domain in enumerate(chosen_domains):
        opt_text = q_source["options"][domain]
        options.append({
            "id": f"q{q_id}_opt{i+1}",
            "text": opt_text,
            "domain": domain
        })
        
    questions.append({
        "id": f"q{q_id}",
        "text": q_source["text"],
        "category": "General",
        "options": options
    })

# Save questions to JSON
with open('quiz_150_questions.json', 'w') as f:
    json.dump(questions, f, indent=2)

print(f"Successfully generated {len(questions)} high-quality custom questions in quiz_150_questions.json.")

# Generate synthetic training dataset
# We'll simulate 5000 users. Each user will be assigned a true underlying domain.
# The user is presented with 10 random questions from the 150-question pool.
# For each question, if their target domain is present in the options, they have a high probability (85%)
# of choosing it. Otherwise they choose randomly.
dataset = []
for _ in range(5000):
    user_true_domain = random.choice(doms)
    asked_questions = random.sample(questions, 10)
    chosen_texts = []
    
    for q in asked_questions:
        matching_opts = [o for o in q['options'] if o['domain'] == user_true_domain]
        if matching_opts and random.random() < 0.85:
            selected = random.choice(matching_opts)
        else:
            # Pick from the other options
            selected = random.choice(q['options'])
            
        chosen_texts.append(selected['text'])
        
    combined_text = " ".join(chosen_texts)
    dataset.append([combined_text, user_true_domain])

# Write to CSV
with open('quiz_training_data.csv', 'w', newline='', encoding='utf-8') as f:
    writer = csv.writer(f, delimiter='\t')
    writer.writerow(["CombinedAnswers", "RecommendedDomain"])
    writer.writerows(dataset)

print(f"Successfully generated 5000 synthetic responses in quiz_training_data.csv.")
