import { AnonymousLayout, MainLayout, ProjectUserManagementContainer, WorkspaceDetailsContainer, WorkspaceUserManagementContainer } from "../components";
import NotificationContainer from "../components/notificationContainer/NotificationContainer";
import ProjectCreationContainer from "../components/projectCreationContainer/ProjectCreationContainer";
import ProjectDetailsContainer from "../components/projectDetailsContainer/ProjectDetailsContainer";
import ProjectManagementContainer from "../components/projectManagementContainer/ProjectManagementContainer";
import ProjectWorktypeDetailsContainer from "../components/projectWorktypeDetailsContainer/ProjectWorktypeDetailsContainer";
import TimeLoggingSearchContainer from "../components/timeLoggingSearchContainer/TimeLoggingSearchContainer";
import { 
  TimeLoggingPage,
  LoginPage, 
  RegisterPage,
  ForgotPasswordPage,
  WorkspaceManagementPage,
  WorkspaceCreationPage,
} from "../pages";
import { getUserJwtToken } from "../utils/sessionUtils";
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
    isAuthorized: () => { return !!getUserJwtToken(); },
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
        name: 'workspace user management page',
        title: 'Workspace user management',
        component: WorkspaceUserManagementContainer,
        path: '/workspace/:id/workspace-users/'
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
      {
        name: 'project user management page',
        title: 'Project user management',
        component: ProjectUserManagementContainer,
        path: '/project/:projectid/project-users/'
      },
      {
        name: 'notification page',
        title: 'Notifications',
        component: NotificationContainer,
        path: '/notifications'
      },
      {
        name: 'reports page',
        title: 'Reports',
        component: TimeLoggingSearchContainer,
        path: '/reports'
      },
    ]
  }
];

export const Routes = renderRoutes(routes);