import { AnonymousLayout, MainLayout, WorkspaceDetailsContainer } from "../components";
import ProjectCreationContainer from "../components/projectCreationContainer/ProjectCreationContainer";
import ProjectDetailsContainer from "../components/projectDetailsContainer/ProjectDetailsContainer";
import ProjectManagementContainer from "../components/projectManagementContainer/ProjectManagementContainer";
import ProjectWorktypeDetailsContainer from "../components/projectWorktypeDetailsContainer/ProjectWorktypeDetailsContainer";
import { 
  TimeLoggingPage,
  LoginPage, 
  RegisterPage,
  ForgotPasswordPage,
  ClientManagementPage, 
  AccountManagementPage,
  ClientDataPage,
  WorkspaceManagementPage,
  WorkspaceCreationPage,
} from "../pages";
import { getUserAuthToken } from "../utils/sessionUtils";
import { renderRoutes } from "./generate-routes";

export const routes = [
{
    layout: AnonymousLayout,
    isPublic: true,
    routes: [
      {
        name: 'login',
        title: 'Login page',
        component: LoginPage,
        path: '/login',
      },
      {
        name: 'register',
        title: 'Registration page',
        component: RegisterPage,
        path: '/register',
      },
      {
        name: 'forgot password',
        title: 'Forgot password page',
        component: ForgotPasswordPage,
        path: '/forgot-password',
      }
    ]
},
{
    layout: MainLayout,
    isAuthorized: () => { return !!getUserAuthToken(); },
    routes: [
      {
        name: 'timeLogging',
        title: 'TimeLogging',
        component: TimeLoggingPage,
        path: '/',
      },
      {
        name: 'workspace management page',
        title: 'Workspace management',
        component: WorkspaceManagementPage,
        path: '/workspace-management',
      },
      {
        name: 'workspace create page',
        title: 'Workspace creation',
        component: WorkspaceCreationPage,
        path: '/workspace-creation',
      },
      {
        name: 'workspace details page',
        title: 'Workspace details',
        component: WorkspaceDetailsContainer,
        path: '/workspace/:id',
      },
      {
        name: 'project management page',
        title: 'Project management',
        component: ProjectManagementContainer,
        path: '/project-management'
      },
      {
        name: 'project create page',
        title: 'Project creation',
        component: ProjectCreationContainer,
        path: '/project-creation'
      },
      {
        name: 'project details page',
        title: 'Project details',
        component: ProjectDetailsContainer,
        path: '/project/:id'
      },
      {
        name: 'project worktype details page',
        title: 'Project worktype details',
        component: ProjectWorktypeDetailsContainer,
        path: '/project/:projectid/project-worktype/:id'
      },


      // {
      //   name: 'analyticsPage',
      //   title: 'Analytics page',
      //   component: AnalyticsPage,
      //   path: '/analytics'
      // },
      {
        name: 'client management page',
        title: 'Client management',
        component: ClientManagementPage,
        path: '/client-management'
      },
      {
        name: 'client data page',
        title: 'Client data',
        component: ClientDataPage,
        path: '/client-data'
      },
      {
        name: 'Account management page',
        title: 'Account management page',
        component: AccountManagementPage,
        path: '/account-management'
      },
    ]
  }
];

export const Routes = renderRoutes(routes);